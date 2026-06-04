using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using DummyJson.Domain.Carts;
using DummyJson.Domain.Comments;
using DummyJson.Domain.Posts;
using DummyJson.Domain.Products;
using DummyJson.Domain.Quotes;
using DummyJson.Domain.Recipes;
using DummyJson.Domain.Todos;
using DummyJson.Domain.Users;
using DummyJson.Persistence.Context;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace DummyJson.Persistence.Seeding;

/// <summary>
/// Seeds both EF Core (PostgreSQL) and MongoDB databases from local JSON files on first run.
/// Detects whether each JSON file is a flat array <c>[...]</c> or a keyed wrapper
/// <c>{ "users": [...] }</c> automatically, so no manual mapping is required.
///
/// When <paramref name="seedImages"/> is <c>true</c>, avatar and product images
/// are downloaded from the URLs in the JSON and saved under
/// <c>wwwroot/images/users/</c> and <c>wwwroot/images/products/</c>.
/// The stored URL on the entity is then rewritten to the local relative path.
/// </summary>
public sealed class DataSeeder
{
    private readonly AppDbContext _dbContext;
    private readonly MongoDbContext _mongoContext;
    private readonly ILogger<DataSeeder> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public DataSeeder(
        AppDbContext dbContext,
        MongoDbContext mongoContext,
        ILogger<DataSeeder> logger,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _dbContext = dbContext;
        _mongoContext = mongoContext;
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // ── Entry Point ───────────────────────────────────────────────────────────

    /// <summary>
    /// Runs all seed operations. Safe to call on every startup — each step
    /// checks if data already exists before inserting.
    /// </summary>
    /// <param name="seedDataPath">Directory containing the *.json seed files.</param>
    /// <param name="wwwRootPath">
    ///   Physical path to the wwwroot folder. Images will be saved under
    ///   <c>{wwwRootPath}/images/users/</c> and <c>{wwwRootPath}/images/products/</c>.
    /// </param>
    /// <param name="seedImages">
    ///   When <c>true</c>, image URLs are downloaded and stored locally.
    ///   Defaults to <c>false</c> to keep startup fast.
    /// </param>
    /// <param name="cancellationToken">Propagated to all async DB calls.</param>
    public async Task SeedAsync(
        string seedDataPath,
        string wwwRootPath,
        bool seedImages = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting database seeding from {Path} (images={SeedImages})",
            seedDataPath, seedImages);

        // Ensure the relational schema is up-to-date
        await _dbContext.Database.MigrateAsync(cancellationToken);

        await SeedIdentityAsync(cancellationToken);
        await SeedUsersAsync(seedDataPath, wwwRootPath, seedImages, cancellationToken);
        await SeedTodosAsync(seedDataPath, cancellationToken);
        await SeedCartsAsync(seedDataPath, cancellationToken);
        await SeedQuotesAsync(seedDataPath, cancellationToken);
        await SeedCommentsAsync(seedDataPath, cancellationToken);
        await SeedRecipesAsync(seedDataPath, wwwRootPath, seedImages, cancellationToken);
        await SeedProductsAsync(seedDataPath, wwwRootPath, seedImages, cancellationToken);
        await SeedPostsAsync(seedDataPath, cancellationToken);

        _logger.LogInformation("Database seeding completed.");
    }

    // ── Identity ──────────────────────────────────────────────────────────────

    private async Task SeedIdentityAsync(CancellationToken ct)
    {
        var roles = new[]
        {
            new ApplicationRole("Developer") { Priority = 3 },
            new ApplicationRole("Admin") { Priority = 2 },
            new ApplicationRole("System") { Priority = 1 }
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role.Name!))
                await _roleManager.CreateAsync(role);
        }

        if (await _userManager.FindByEmailAsync("developer@dummy.com") is null)
        {
            var dev = new ApplicationUser
            {
                UserName = "developer",
                Email = "developer@dummy.com",
                FirstName = "Dev",
                DomainUserId = Guid.CreateVersion7()
            };
            await _userManager.CreateAsync(dev, "Dev@1234");
            await _userManager.AddToRoleAsync(dev, "Developer");
        }

        if (await _userManager.FindByEmailAsync("admin@dummy.com") is null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@dummy.com",
                FirstName = "Admin",
                DomainUserId = Guid.CreateVersion7()
            };
            await _userManager.CreateAsync(admin, "Admin@123");
            await _userManager.AddToRoleAsync(admin, "Admin");
        }

        _logger.LogInformation("Identity roles and users seeded.");
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    private async Task SeedUsersAsync(
        string path,
        string wwwRoot,
        bool seedImages,
        CancellationToken ct)
    {
        if (await _dbContext.DomainUsers.AnyAsync(ct))
        {
            _logger.LogInformation("Users already seeded, skipping.");
            return;
        }

        var dtos = LoadJson<UserSeedDto>("users.json", path, "users");
        if (dtos is null || dtos.Count == 0) return;

        var imageDir = Path.Combine(wwwRoot, "images", "users");
        if (seedImages) EnsureDirectory(imageDir);

        var users = new List<User>(dtos.Count);
        foreach (var dto in dtos)
        {
            string? localImage = dto.Image;

            if (seedImages && !string.IsNullOrWhiteSpace(dto.Image))
            {
                var filename = $"{SanitizeFilename(dto.Username)}.jpg";
                localImage = await DownloadImageAsync(dto.Image, imageDir, filename, ct)
                    ?? dto.Image;
            }

            var result = User.Create(
                dto.FirstName, dto.LastName, dto.Username, dto.Email,
                dto.Phone ?? "", localImage, dto.Gender, dto.BirthDate);

            if (result.IsSuccess)
                users.Add(result.Value);
        }

        // Bulk insert — bypasses change-tracking, no SaveChangesAsync needed
        await _dbContext.BulkInsertAsync(users, new BulkConfig
        {
            BatchSize = 500,
            PreserveInsertOrder = true,
            SetOutputIdentity = false
        }, cancellationToken: ct);

        _logger.LogInformation("Seeded {Count} users.", users.Count);
    }

    // ── Todos ─────────────────────────────────────────────────────────────────

    private async Task SeedTodosAsync(string path, CancellationToken ct)
    {
        if (await _dbContext.Todos.AnyAsync(ct))
        {
            _logger.LogInformation("Todos already seeded, skipping.");
            return;
        }

        var dtos = LoadJson<TodoSeedDto>("todos.json", path, "todos");
        if (dtos is null || dtos.Count == 0) return;

        var todos = new List<Todo>(dtos.Count);
        foreach (var dto in dtos)
        {
            var result = Todo.Create(Guid.CreateVersion7(), dto.Todo);
            if (!result.IsSuccess) continue;

            if (dto.Completed) result.Value.Complete();
            todos.Add(result.Value);
        }

        await _dbContext.BulkInsertAsync(todos, new BulkConfig
        {
            BatchSize = 500,
            PreserveInsertOrder = true,
            SetOutputIdentity = false
        }, cancellationToken: ct);

        _logger.LogInformation("Seeded {Count} todos.", todos.Count);
    }

    // ── Carts ─────────────────────────────────────────────────────────────────

    private async Task SeedCartsAsync(string path, CancellationToken ct)
    {
        if (await _dbContext.Carts.AnyAsync(ct))
        {
            _logger.LogInformation("Carts already seeded, skipping.");
            return;
        }

        var dtos = LoadJson<CartSeedDto>("carts.json", path, "carts");
        if (dtos is null || dtos.Count == 0) return;

        var carts = new List<Cart>(dtos.Count);
        var cartItems = new List<CartItem>();

        foreach (var dto in dtos)
        {
            var cart = Cart.Create(Guid.CreateVersion7());

            foreach (var item in dto.Products ?? [])
            {
                var addResult = cart.AddItem(
                    Guid.CreateVersion7(),
                    item.Title ?? "",
                    item.Price,
                    item.DiscountPercentage,
                    item.Quantity);

                // collect CartItems for bulk insert
                _ = addResult;
            }

            carts.Add(cart);
        }

        // Insert carts first (parent), then items (children)
        await _dbContext.BulkInsertAsync(carts, new BulkConfig
        {
            BatchSize = 500,
            PreserveInsertOrder = true,
            SetOutputIdentity = false,
            IncludeGraph = true   // cascade insert navigation properties (CartItems)
        }, cancellationToken: ct);

        _logger.LogInformation("Seeded {Count} carts.", carts.Count);
    }

    // ── Products (MongoDB) ────────────────────────────────────────────────────

    private async Task SeedProductsAsync(
        string path,
        string wwwRoot,
        bool seedImages,
        CancellationToken ct)
    {
        var count = await _mongoContext.Products
            .CountDocumentsAsync(FilterDefinition<Product>.Empty, null, ct);

        if (count > 0)
        {
            _logger.LogInformation("Products already seeded, skipping.");
            return;
        }

        var dtos = LoadJson<ProductSeedDto>("products.json", path, "products");
        if (dtos is null || dtos.Count == 0) return;

        var thumbDir = Path.Combine(wwwRoot, "images", "products");
        if (seedImages) EnsureDirectory(thumbDir);

        var products = new List<Product>(dtos.Count);

        foreach (var dto in dtos)
        {
            string thumbnail = dto.Thumbnail;
            var localImages = new List<string>(dto.Images ?? []);

            if (seedImages)
            {
                var sku = SanitizeFilename(dto.Sku ?? dto.Title);

                // Download thumbnail
                if (!string.IsNullOrWhiteSpace(dto.Thumbnail))
                {
                    var thumbFile = $"{sku}-thumb.jpg";
                    thumbnail = await DownloadImageAsync(dto.Thumbnail, thumbDir, thumbFile, ct)
                        ?? dto.Thumbnail;
                }

                // Download product images
                for (var i = 0; i < localImages.Count; i++)
                {
                    var imgUrl = localImages[i];
                    if (string.IsNullOrWhiteSpace(imgUrl)) continue;

                    var imgFile = $"{sku}-{i}.jpg";
                    localImages[i] = await DownloadImageAsync(imgUrl, thumbDir, imgFile, ct)
                        ?? imgUrl;
                }
            }

            var result = Product.Create(
                dto.Title, dto.Description, dto.Price, dto.DiscountPercentage,
                dto.Stock, dto.Brand ?? "", dto.Category, thumbnail,
                localImages, dto.Tags ?? [], dto.Sku ?? "",
                dto.Meta?.Barcode ?? "", dto.MinimumOrderQuantity,
                dto.WarrantyInformation ?? "", dto.ShippingInformation ?? "",
                dto.AvailabilityStatus ?? "In Stock", dto.ReturnPolicy ?? "");

            if (result.IsSuccess)
                products.Add(result.Value);
        }

        if (products.Count > 0)
            await _mongoContext.Products.InsertManyAsync(products, null, ct);

        _logger.LogInformation("Seeded {Count} products.", products.Count);
    }

    // ── Posts (MongoDB) ───────────────────────────────────────────────────────

    private async Task SeedPostsAsync(string path, CancellationToken ct)
    {
        var count = await _mongoContext.Posts
            .CountDocumentsAsync(FilterDefinition<Post>.Empty, null, ct);

        if (count > 0)
        {
            _logger.LogInformation("Posts already seeded, skipping.");
            return;
        }

        var dtos = LoadJson<PostSeedDto>("posts.json", path, "posts");
        if (dtos is null || dtos.Count == 0) return;

        var posts = new List<Post>(dtos.Count);
        foreach (var dto in dtos)
        {
            var result = Post.Create(Guid.CreateVersion7(), dto.Title, dto.Body, dto.Tags ?? []);
            if (result.IsSuccess)
                posts.Add(result.Value);
        }

        if (posts.Count > 0)
            await _mongoContext.Posts.InsertManyAsync(posts, null, ct);

        _logger.LogInformation("Seeded {Count} posts.", posts.Count);
    }

    // ── Quotes ────────────────────────────────────────────────────────────────

    private async Task SeedQuotesAsync(string path, CancellationToken ct)
    {
        if (await _dbContext.Quotes.AnyAsync(ct))
        {
            _logger.LogInformation("Quotes already seeded, skipping.");
            return;
        }

        var dtos = LoadJson<QuoteSeedDto>("quotes.json", path, "quotes");
        if (dtos is null || dtos.Count == 0) return;

        var quotes = new List<Quote>(dtos.Count);
        foreach (var dto in dtos)
        {
            var result = Quote.Create(dto.Quote, dto.Author);
            if (result.IsSuccess)
                quotes.Add(result.Value);
        }

        await _dbContext.BulkInsertAsync(quotes, new BulkConfig
        {
            BatchSize = 500,
            PreserveInsertOrder = true,
            SetOutputIdentity = false
        }, cancellationToken: ct);

        _logger.LogInformation("Seeded {Count} quotes.", quotes.Count);
    }

    // ── Comments ──────────────────────────────────────────────────────────────

    private async Task SeedCommentsAsync(string path, CancellationToken ct)
    {
        if (await _dbContext.Comments.AnyAsync(ct))
        {
            _logger.LogInformation("Comments already seeded, skipping.");
            return;
        }

        var dtos = LoadJson<CommentSeedDto>("comments.json", path, "comments");
        if (dtos is null || dtos.Count == 0) return;

        var comments = new List<Comment>(dtos.Count);
        foreach (var dto in dtos)
        {
            // PostId cannot be correlated during seeding (DummyJSON uses integer IDs);
            // we leave it null so the row is still queryable by username / body.
            var result = Comment.Create(
                dto.Body,
                postId: null,
                username: dto.User?.Username ?? "",
                fullName: dto.User?.FullName ?? "",
                likes: dto.Likes);

            if (result.IsSuccess)
                comments.Add(result.Value);
        }

        await _dbContext.BulkInsertAsync(comments, new BulkConfig
        {
            BatchSize = 500,
            PreserveInsertOrder = true,
            SetOutputIdentity = false
        }, cancellationToken: ct);

        _logger.LogInformation("Seeded {Count} comments.", comments.Count);
    }

    // ── Recipes ───────────────────────────────────────────────────────────────

    private async Task SeedRecipesAsync(
        string path,
        string wwwRoot,
        bool seedImages,
        CancellationToken ct)
    {
        if (await _dbContext.Recipes.AnyAsync(ct))
        {
            _logger.LogInformation("Recipes already seeded, skipping.");
            return;
        }

        var dtos = LoadJson<RecipeSeedDto>("recipes.json", path, "recipes");
        if (dtos is null || dtos.Count == 0) return;

        var imageDir = Path.Combine(wwwRoot, "images", "recipes");
        if (seedImages) EnsureDirectory(imageDir);

        var recipes = new List<Recipe>(dtos.Count);
        foreach (var dto in dtos)
        {
            string? localImage = dto.Image;

            if (seedImages && !string.IsNullOrWhiteSpace(dto.Image))
            {
                // Keep original extension (.webp, .jpg, etc.)
                var ext = Path.GetExtension(new Uri(dto.Image).LocalPath);
                if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
                var filename = $"{SanitizeFilename(dto.Name)}{ext}";
                localImage = await DownloadImageAsync(dto.Image, imageDir, filename, ct)
                    ?? dto.Image;
            }

            var result = Recipe.Create(
                dto.Name,
                dto.Ingredients ?? [],
                dto.Instructions ?? [],
                dto.PrepTimeMinutes,
                dto.CookTimeMinutes,
                dto.Servings,
                dto.Difficulty ?? "Easy",
                dto.Cuisine ?? "",
                dto.CaloriesPerServing,
                dto.Tags ?? [],
                dto.MealType ?? [],
                localImage,
                dto.Rating,
                dto.ReviewCount);

            if (result.IsSuccess)
                recipes.Add(result.Value);
        }

        await _dbContext.BulkInsertAsync(recipes, new BulkConfig
        {
            BatchSize = 500,
            PreserveInsertOrder = true,
            SetOutputIdentity = false
        }, cancellationToken: ct);

        _logger.LogInformation("Seeded {Count} recipes.", recipes.Count);
    }

    // ── JSON Loader ───────────────────────────────────────────────────────────

    /// <summary>
    /// Reads a JSON file and deserializes it. Handles two formats:
    /// <list type="bullet">
    ///   <item><description>Flat array: <c>[{ ... }, ...]</c></description></item>
    ///   <item><description>Keyed wrapper: <c>{ "users": [{ ... }, ...] }</c></description></item>
    /// </list>
    /// </summary>
    private List<T>? LoadJson<T>(string filename, string directory, string key)
    {
        var file = Path.Combine(directory, filename);
        if (!File.Exists(file))
        {
            _logger.LogWarning("{File} not found in {Dir}, skipping.", filename, directory);
            return null;
        }

        var json = File.ReadAllText(file);
        var trimmed = json.TrimStart();

        try
        {
            if (trimmed.StartsWith('['))
            {
                // Flat array — e.g. users.json = [{ ... }]
                return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions);
            }

            // Keyed object — e.g. { "products": [...] }
            var doc = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);

            if (doc.TryGetProperty(key, out var arr))
                return arr.Deserialize<List<T>>(_jsonOptions);

            // Fallback: try every property that is an array
            foreach (var prop in doc.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Array)
                    return prop.Value.Deserialize<List<T>>(_jsonOptions);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {File}.", filename);
        }

        return null;
    }

    // ── Image Download ────────────────────────────────────────────────────────

    /// <summary>
    /// Downloads an image URL to <paramref name="directory"/>/<paramref name="filename"/>.
    /// Returns the local relative URL path (<c>/images/...</c>) on success,
    /// or <c>null</c> on failure so the caller can fall back to the original URL.
    /// </summary>
    private static async Task<string?> DownloadImageAsync(
        string url,
        string directory,
        string filename,
        CancellationToken ct)
    {
        var dest = Path.Combine(directory, filename);

        // Already downloaded on a previous run — skip
        if (File.Exists(dest))
        {
            // Compute relative URL from the directory path
            return ToRelativeUrl(directory, filename);
        }

        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("image/*"));

            var bytes = await http.GetByteArrayAsync(url, ct);
            await File.WriteAllBytesAsync(dest, bytes, ct);

            return ToRelativeUrl(directory, filename);
        }
        catch (Exception)
        {
            // Non-fatal: log at debug level and fall back to the original URL
            return null;
        }
    }

    /// <summary>
    /// Converts an absolute file-system path pair to a web-relative URL.
    /// E.g. <c>wwwroot/images/users</c> + <c>emily.jpg</c>
    ///      → <c>/images/users/emily.jpg</c>
    /// </summary>
    private static string ToRelativeUrl(string directory, string filename)
    {
        // Split on "wwwroot" to get the portion after it
        const string marker = "wwwroot";
        var idx = directory.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            var relative = directory[(idx + marker.Length)..].Replace('\\', '/');
            return $"{relative}/{filename}";
        }

        // Fallback — just return the filename
        return $"/{filename}";
    }

    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    private static string SanitizeFilename(string input)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(input.Select(c => invalid.Contains(c) ? '_' : c).ToArray())
            .Replace(' ', '-')
            .ToLowerInvariant();
    }

    // ── Seed DTOs ──────────────────────────────────────────────────────────────

    private sealed record UserSeedDto(
        string FirstName,
        string LastName,
        string Username,
        string Email,
        string? Phone,
        string? Image,
        string? Gender,
        DateOnly? BirthDate,
        string? Role);

    private sealed record ProductSeedDto(
        string Title,
        string Description,
        decimal Price,
        decimal DiscountPercentage,
        int Stock,
        string? Brand,
        string Category,
        string Thumbnail,
        List<string>? Images,
        List<string>? Tags,
        string? Sku,
        string? WarrantyInformation,
        string? ShippingInformation,
        string? AvailabilityStatus,
        string? ReturnPolicy,
        int MinimumOrderQuantity,
        ProductMetaSeedDto? Meta);

    private sealed record ProductMetaSeedDto(string? Barcode);

    private sealed record PostSeedDto(string Title, string Body, List<string>? Tags);

    private sealed record TodoSeedDto(string Todo, bool Completed);

    private sealed record CartSeedDto(List<CartProductSeedDto>? Products);

    private sealed record CartProductSeedDto(
        string? Title,
        decimal Price,
        decimal DiscountPercentage,
        int Quantity);

    private sealed record QuoteSeedDto(string Quote, string Author);

    private sealed record CommentSeedDto(
        string Body,
        int Likes,
        CommentUserSeedDto? User);

    private sealed record CommentUserSeedDto(
        string Username,
        string FullName);

    private sealed record RecipeSeedDto(
        string Name,
        List<string>? Ingredients,
        List<string>? Instructions,
        int PrepTimeMinutes,
        int CookTimeMinutes,
        int Servings,
        string? Difficulty,
        string? Cuisine,
        int CaloriesPerServing,
        List<string>? Tags,
        List<string>? MealType,
        string? Image,
        double Rating,
        int ReviewCount);
}
