using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using DummyJson.Domain.Carts;
using DummyJson.Domain.Comments;
using DummyJson.Domain.Posts;
using DummyJson.Domain.Products;
using DummyJson.Domain.Quotes;
using DummyJson.Domain.Recipes;
using DummyJson.Domain.Tags;
using DummyJson.Domain.Todos;
using DummyJson.Domain.Users;
using DummyJson.Persistence.Context;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using DomainTag = DummyJson.Domain.Tags.Tag;

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
    private string? _systemUserId;

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
        await SeedRecipesAsync(seedDataPath, wwwRootPath, seedImages, cancellationToken);

        // Relational shadow tables must be seeded FIRST so Products have CategoryIds
        await SeedProductCategoriesAsync(seedDataPath, cancellationToken);
        await SeedTagsAsync(seedDataPath, cancellationToken);

        await SeedProductsAsync(seedDataPath, wwwRootPath, seedImages, cancellationToken);
        await SeedPostsAsync(seedDataPath, cancellationToken);
        await SeedCommentsAsync(seedDataPath, cancellationToken);

        _logger.LogInformation("Database seeding completed.");
    }

    // ── Identity ──────────────────────────────────────────────────────────────

    private async Task SeedIdentityAsync(CancellationToken ct)
    {
        var roles = new[]
        {
            new ApplicationRole("Developer") { Priority = 4 },
            new ApplicationRole("Admin") { Priority = 3 },
            new ApplicationRole("System") { Priority = 2 },
            new ApplicationRole("User") { Priority = 1 }
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role.Name!))
                await _roleManager.CreateAsync(role);
        }

        if (await _userManager.FindByEmailAsync("developer@dummy.com") is null)
        {
            var userResult = ApplicationUser.Create("Dev", "", "developer", "developer@dummy.com", "");
            if (userResult.IsSuccess)
            {
                var dev = userResult.Value;
                dev.Id = Guid.CreateVersion7(); // Use a new ID or rely on Identity generating it
                await _userManager.CreateAsync(dev, "Dev@1234");
                await _userManager.AddToRoleAsync(dev, "Developer");
            }
        }

        if (await _userManager.FindByEmailAsync("admin@dummy.com") is null)
        {
            var userResult = ApplicationUser.Create("Admin", "", "admin", "admin@dummy.com", "");
            if (userResult.IsSuccess)
            {
                var admin = userResult.Value;
                admin.Id = Guid.CreateVersion7();
                await _userManager.CreateAsync(admin, "Admin@123");
                await _userManager.AddToRoleAsync(admin, "Admin");
            }
        }
        var sysUser = await _userManager.FindByEmailAsync("system@dummy.com");
        if (sysUser is null)
        {
            var userResult = ApplicationUser.Create("System", "", "system", "system@dummy.com", "");
            if (userResult.IsSuccess)
            {
                var system = userResult.Value;
                system.Id = Guid.CreateVersion7();
                await _userManager.CreateAsync(system, "System@123");
                await _userManager.AddToRoleAsync(system, "System");
                _systemUserId = system.Id.ToString();
            }
        }
        else
        {
            _systemUserId = sysUser.Id.ToString();
        }

        _logger.LogInformation("Identity roles and users seeded.");
    }

    private void SetAuditFields(IEnumerable<object> entities)
    {
        if (_systemUserId == null) return;
        foreach (var entity in entities)
        {
            if (entity is DummyJson.Domain.Common.Interfaces.IAuditable auditable)
            {
                var type = entity.GetType();
                type.GetProperty(nameof(DummyJson.Domain.Common.Interfaces.IAuditable.CreatedBy))?.SetValue(entity, _systemUserId);
                type.GetProperty(nameof(DummyJson.Domain.Common.Interfaces.IAuditable.UpdatedAt))?.SetValue(entity, null);
            }
        }
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    private async Task SeedUsersAsync(
        string path,
        string wwwRoot,
        bool seedImages,
        CancellationToken ct)
    {
        if (await _dbContext.Users.CountAsync(ct) >=4)
        {
            _logger.LogInformation("Users already seeded, skipping.");
            return;
        }

        var dtos = LoadJson<UserSeedDto>("users.json", path, "users");
        if (dtos is null || dtos.Count == 0) return;

        var imageDir = Path.Combine(wwwRoot, "images", "users");
        if (seedImages) EnsureDirectory(imageDir);

        var userRole = await _roleManager.FindByNameAsync("User");
        var userRoleId = userRole?.Id ?? Guid.Empty;

        var users = new List<ApplicationUser>(dtos.Count);
        var userRoles = new List<IdentityUserRole<Guid>>();
        var userAddresses = new List<UserAddress>();
        var userPreferences = new List<UserPreferences>();
        var hasher = new PasswordHasher<ApplicationUser>();

        foreach (var dto in dtos)
        {
            string? localImage = dto.Image;

            if (seedImages && !string.IsNullOrWhiteSpace(dto.Image))
            {
                var filename = $"{SanitizeFilename(dto.Username)}.jpg";
                localImage = await DownloadImageAsync(dto.Image, imageDir, filename, ct)
                    ?? dto.Image;
            }

            DateOnly? parsedBirthDate = null;
            if (DateOnly.TryParse(dto.BirthDate, out var bDate))
                parsedBirthDate = bDate;

            var result = ApplicationUser.Create(
                dto.FirstName, dto.LastName, dto.Username, dto.Email,
                dto.Phone ?? "", dto.Gender, parsedBirthDate);

            if (result.IsSuccess)
            {
                var user = result.Value;
                user.PasswordHash = hasher.HashPassword(user, "User@1234");
                user.NormalizedEmail = user.Email?.ToUpperInvariant();
                user.NormalizedUserName = user.UserName?.ToUpperInvariant();
                user.SecurityStamp = Guid.NewGuid().ToString();

                users.Add(user);

                if (userRoleId != Guid.Empty)
                {
                    userRoles.Add(new IdentityUserRole<Guid> { UserId = user.Id, RoleId = userRoleId });
                }

                if (dto.Address is not null)
                {
                    userAddresses.Add(new UserAddress
                    {
                        UserId = user.Id,
                        Street = dto.Address.Address,
                        City = dto.Address.City,
                        State = dto.Address.State,
                        PostalCode = dto.Address.PostalCode,
                        Country = dto.Address.Country,
                        Latitude = dto.Address.Coordinates?.Lat,
                        Longitude = dto.Address.Coordinates?.Lng
                    });
                }

                // Add UserPreferences with Image
                var prefs = UserPreferences.Create(user.Id, image: localImage);
                userPreferences.Add(prefs);
            }
        }

        SetAuditFields(users);

        // Bulk insert — bypasses change-tracking, no SaveChangesAsync needed
        await _dbContext.BulkInsertAsync(users, new BulkConfig
        {
            BatchSize = 500,
            PreserveInsertOrder = true,
            SetOutputIdentity = false
        }, cancellationToken: ct);

        if (userRoles.Count > 0)
            await _dbContext.BulkInsertAsync(userRoles, new BulkConfig { BatchSize = 500 }, cancellationToken: ct);

        if (userAddresses.Count > 0)
            await _mongoContext.UserAddresses.InsertManyAsync(userAddresses, null, ct);

        if (userPreferences.Count > 0)
            await _mongoContext.UserPreferences.InsertManyAsync(userPreferences, null, ct);

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

        SetAuditFields(todos);
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

        SetAuditFields(carts);
        await _dbContext.BulkInsertAsync(carts, new BulkConfig
        {
            BatchSize = 500,
            PreserveInsertOrder = true,
            SetOutputIdentity = false
        }, cancellationToken: ct);

        var allItems = carts.SelectMany(c => c.Items).ToList();
        if (allItems.Count > 0)
        {
            await _dbContext.BulkInsertAsync(allItems, new BulkConfig
            {
                BatchSize = 1000,
                PreserveInsertOrder = true,
                SetOutputIdentity = false
            }, cancellationToken: ct);
        }

        _logger.LogInformation("Seeded {Count} carts.", carts.Count);
    }

    // ── Products (MongoDB) ────────────────────────────────────────────────────

    private async Task SeedProductsAsync(
        string path,
        string wwwRoot,
        bool seedImages,
        CancellationToken ct)
    {
        if (await _dbContext.Products.AnyAsync(ct))
        {
            _logger.LogInformation("Products already seeded, skipping.");
            return;
        }

        var dtos = LoadJson<ProductSeedDto>("products.json", path, "products");
        if (dtos is null || dtos.Count == 0) return;

        var categories = await _dbContext.ProductCategories.ToDictionaryAsync(c => c.Slug, c => c.Id, ct);
        
        var tags = await _mongoContext.Tags.Find(_ => true).ToListAsync(ct);
        var tagsMap = tags.ToDictionary(t => t.Name, t => t.Id, StringComparer.OrdinalIgnoreCase);

        var products = new List<Product>(dtos.Count);

        var thumbDir = Path.Combine(wwwRoot, "images", "products");
        if (seedImages) EnsureDirectory(thumbDir);

        var productImages = new List<ProductImage>();
        var productTags = new List<ProductTag>();
        var productReviews = new List<ProductReview>();
        
        var userIds = await _dbContext.Users.Select(u => u.Id).ToListAsync(ct);
        var random = new Random();

        foreach (var dto in dtos)
        {
            string thumbnail = dto.Thumbnail;
            var localImages = new List<string>(dto.Images ?? []);

            if (seedImages)
            {
                var sku = SanitizeFilename(dto.Sku ?? dto.Title);
                if (!string.IsNullOrWhiteSpace(dto.Thumbnail))
                {
                    var thumbFile = $"{sku}-thumb.jpg";
                    thumbnail = await DownloadImageAsync(dto.Thumbnail, thumbDir, thumbFile, ct) ?? dto.Thumbnail;
                }
                for (var i = 0; i < localImages.Count; i++)
                {
                    var imgUrl = localImages[i];
                    if (string.IsNullOrWhiteSpace(imgUrl)) continue;
                    var imgFile = $"{sku}-{i}.jpg";
                    localImages[i] = await DownloadImageAsync(imgUrl, thumbDir, imgFile, ct) ?? imgUrl;
                }
            }

            var categoryId = categories.GetValueOrDefault(dto.Category ?? "", Guid.Empty);

            var result = Product.Create(
                dto.Title, dto.Description, dto.Price, dto.DiscountPercentage,
                dto.Stock, dto.Brand ?? "", categoryId, thumbnail,
                dto.Sku ?? "", dto.Meta?.Barcode ?? "", dto.MinimumOrderQuantity,
                dto.WarrantyInformation ?? "", dto.ShippingInformation ?? "",
                dto.AvailabilityStatus ?? "In Stock", dto.ReturnPolicy ?? "");

            if (result.IsSuccess)
            {
                products.Add(result.Value);
                
                // Add ProductImages to MongoDB list
                foreach (var imgUrl in localImages)
                {
                    productImages.Add(new ProductImage(result.Value.Id, imgUrl));
                }

                // Add ProductTags to MongoDB list
                if (dto.Tags is not null)
                {
                    foreach (var tagName in dto.Tags)
                    {
                        if (tagsMap.TryGetValue(tagName, out var tagId))
                            productTags.Add(new ProductTag(result.Value.Id, tagId));
                    }
                }

                // Add ProductReviews to MongoDB list
                if (dto.Reviews is not null)
                {
                    foreach (var r in dto.Reviews)
                    {
                        var randomUserId = userIds.Count > 0 ? userIds[random.Next(userIds.Count)] : Guid.Empty;
                        var review = new ProductReview(
                            result.Value.Id,
                            randomUserId,
                            r.Rating,
                            r.Comment ?? string.Empty
                        );
                        // If we wanted to preserve the exact seed date, we'd add a setter or reflection.
                        // For now, ProductReview sets Date = UtcNow in its constructor.
                        productReviews.Add(review);
                    }
                }
            }
        }

        if (products.Count > 0)
        {
            SetAuditFields(products);
            await _dbContext.BulkInsertAsync(products, new BulkConfig { BatchSize = 500, PreserveInsertOrder = true }, cancellationToken: ct);
            if (productTags.Count > 0)
                await _mongoContext.ProductTags.InsertManyAsync(productTags, null, ct);
            if (productImages.Count > 0)
                await _mongoContext.ProductImages.InsertManyAsync(productImages, null, ct);
            if (productReviews.Count > 0)
                await _mongoContext.ProductReviews.InsertManyAsync(productReviews, null, ct);
        }

        _logger.LogInformation("Seeded {Count} products and their images.", products.Count);
    }

    // ── Posts (MongoDB) ───────────────────────────────────────────────────────

    private async Task SeedPostsAsync(string path, CancellationToken ct)
    {
        if (await _dbContext.Posts.AnyAsync(ct))
        {
            _logger.LogInformation("Posts already seeded, skipping.");
            return;
        }

        var dtos = LoadJson<PostSeedDto>("posts.json", path, "posts");
        if (dtos is null || dtos.Count == 0) return;

        var tags = await _mongoContext.Tags.Find(_ => true).ToListAsync(ct);
        var tagsMap = tags.ToDictionary(t => t.Name, t => t.Id, StringComparer.OrdinalIgnoreCase);

        var posts = new List<Post>(dtos.Count);
        var postTags = new List<PostTag>();

        foreach (var dto in dtos)
        {
            var result = Post.Create(Guid.CreateVersion7(), dto.Title, dto.Body, new List<string>());
            if (result.IsSuccess)
            {
                posts.Add(result.Value);
                if (dto.Tags is not null)
                {
                    foreach (var tagName in dto.Tags)
                    {
                        if (tagsMap.TryGetValue(tagName, out var tagId))
                            postTags.Add(new PostTag(result.Value.Id, tagId));
                    }
                }
            }
        }

        if (posts.Count > 0)
        {
            SetAuditFields(posts);
            await _dbContext.BulkInsertAsync(posts, new BulkConfig { BatchSize = 500, PreserveInsertOrder = true }, cancellationToken: ct);
            if (postTags.Count > 0)
                await _mongoContext.PostTags.InsertManyAsync(postTags, null, ct);
        }

        _logger.LogInformation("Seeded {Count} posts.", posts.Count);
    }

    // ── Quotes ────────────────────────────────────────────────────────────────

    private async Task SeedQuotesAsync(string path, CancellationToken ct)
    {
        var count = await _mongoContext.Quotes.CountDocumentsAsync(FilterDefinition<Quote>.Empty, null, ct);
        if (count > 0)
        {
            _logger.LogInformation("Quotes already seeded in MongoDB, skipping.");
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

        SetAuditFields(quotes);
        if (quotes.Count > 0)
            await _mongoContext.Quotes.InsertManyAsync(quotes, null, ct);

        _logger.LogInformation("Seeded {Count} quotes in MongoDB.", quotes.Count);
    }

    // ── Comments ──────────────────────────────────────────────────────────────

    private async Task SeedCommentsAsync(string path, CancellationToken ct)
    {
        var count = await _mongoContext.Comments.CountDocumentsAsync(FilterDefinition<Comment>.Empty, null, ct);
        if (count > 0)
        {
            _logger.LogInformation("Comments already seeded in MongoDB, skipping.");
            return;
        }

        var dtos = LoadJson<CommentSeedDto>("comments.json", path, "comments");
        if (dtos is null || dtos.Count == 0) return;

        var userIds = await _dbContext.Users.Select(u => u.Id).ToListAsync(ct);
        if (userIds.Count == 0)
        {
            _logger.LogWarning("No users found to assign comments to. Run user seeding first.");
            return;
        }

        var postIds = await _dbContext.Posts.Select(p => p.Id).ToListAsync(ct);

        var random = new Random();
        var comments = new List<Comment>(dtos.Count);
        foreach (var dto in dtos)
        {
            var randomUserId = userIds[random.Next(userIds.Count)];
            var randomPostId = postIds.Count > 0 ? (Guid?)postIds[random.Next(postIds.Count)] : null;
            
            var result = Comment.Create(
                dto.Body,
                postId: randomPostId,
                userId: randomUserId,
                likes: dto.Likes);

            if (result.IsSuccess)
                comments.Add(result.Value);
        }

        SetAuditFields(comments);
        if (comments.Count > 0)
            await _mongoContext.Comments.InsertManyAsync(comments, null, ct);

        _logger.LogInformation("Seeded {Count} comments in MongoDB.", comments.Count);
    }

    // ── Recipes ───────────────────────────────────────────────────────────────

    private async Task SeedRecipesAsync(
        string path,
        string wwwRoot,
        bool seedImages,
        CancellationToken ct)
    {
        var count = await _mongoContext.Recipes.CountDocumentsAsync(FilterDefinition<Recipe>.Empty, null, ct);
        if (count > 0)
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

        if (recipes.Count > 0)
        {
            SetAuditFields(recipes);
            await _mongoContext.Recipes.InsertManyAsync(recipes, null, ct);
        }

        _logger.LogInformation("Seeded {Count} recipes.", recipes.Count);
    }

    // ── Product Categories (PostgreSQL — derived from MongoDB products) ────────

    /// <summary>
    /// Reads distinct category slugs from the MongoDB Products collection and
    /// inserts them into the relational <c>ProductCategories</c> table.
    /// Each slug becomes both the <c>Slug</c> (e.g. "kitchen-accessories") and
    /// a title-cased <c>Name</c> (e.g. "Kitchen Accessories").
    /// Idempotent — skips if any categories already exist.
    /// </summary>
    private async Task SeedProductCategoriesAsync(string path, CancellationToken ct)
    {
        if (await _dbContext.ProductCategories.AnyAsync(ct))
        {
            _logger.LogInformation("ProductCategories already seeded, skipping.");
            return;
        }

        var dtos = LoadJson<ProductSeedDto>("products.json", path, "products");
        if (dtos is null || dtos.Count == 0) return;

        var slugs = dtos.Where(d => !string.IsNullOrWhiteSpace(d.Category))
                        .Select(d => d.Category)
                        .Distinct()
                        .ToList();

        if (slugs.Count == 0) return;

        var categories = new List<ProductCategory>(slugs.Count);
        foreach (var slug in slugs.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct())
        {
            // Convert slug "kitchen-accessories" → "Kitchen Accessories"
            var name = System.Globalization.CultureInfo.InvariantCulture
                .TextInfo.ToTitleCase(slug.Replace('-', ' '));

            var result = ProductCategory.Create(name, slug);
            if (result.IsSuccess)
                categories.Add(result.Value);
        }

        SetAuditFields(categories);
        await _dbContext.BulkInsertAsync(categories, new BulkConfig
        {
            BatchSize = 200,
            PreserveInsertOrder = true,
            SetOutputIdentity = false
        }, cancellationToken: ct);

        _logger.LogInformation("Seeded {Count} product categories.", categories.Count);
    }

    private async Task SeedTagsAsync(string path, CancellationToken ct)
    {
        if (await _mongoContext.Tags.Find(_ => true).AnyAsync(ct))
        {
            _logger.LogInformation("Tags already seeded in MongoDB, skipping.");
            return;
        }

        var productDtos = LoadJson<ProductSeedDto>("products.json", path, "products") ?? [];
        var postDtos = LoadJson<PostSeedDto>("posts.json", path, "posts") ?? [];

        var productTags = productDtos
            .Where(p => p.Tags != null)
            .SelectMany(p => p.Tags!)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var postTags = postDtos
            .Where(p => p.Tags != null)
            .SelectMany(p => p.Tags!)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var tagsDict = new Dictionary<string, TagType>(StringComparer.OrdinalIgnoreCase);

        foreach (var pt in productTags)
            tagsDict[pt] = TagType.Product;

        foreach (var pt in postTags)
        {
            if (tagsDict.ContainsKey(pt))
                tagsDict[pt] = TagType.Shared;
            else
                tagsDict[pt] = TagType.Post;
        }

        var tagsToInsert = new List<DummyJson.Domain.Tags.Tag>(tagsDict.Count);
        foreach (var kvp in tagsDict)
        {
            var result = DummyJson.Domain.Tags.Tag.Create(kvp.Key, kvp.Value);
            if (result.IsSuccess)
                tagsToInsert.Add(result.Value);
        }

        if (tagsToInsert.Count > 0)
        {
            SetAuditFields(tagsToInsert);
            await _mongoContext.Tags.InsertManyAsync(tagsToInsert, null, ct);
        }

        _logger.LogInformation("Seeded {Count} tags in MongoDB.", tagsToInsert.Count);
    }
    // ── Projection DTOs for MongoDB tag queries ───────────────────────────────

    private sealed class ProductTagProjection
    {
        public Guid Id { get; set; }
        public List<string>? Tags { get; set; }
    }

    private sealed class PostTagProjection
    {
        public Guid Id { get; set; }
        public List<string>? Tags { get; set; }
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
        string? BirthDate,
        string? Role,
        UserAddressSeedDto? Address);

    private sealed record UserAddressSeedDto(
        string? Address,
        string? City,
        string? State,
        string? StateCode,
        string? PostalCode,
        CoordinatesSeedDto? Coordinates,
        string? Country);

    private sealed record CoordinatesSeedDto(
        double Lat,
        double Lng);

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
        ProductMetaSeedDto? Meta,
        List<ReviewSeedDto>? Reviews);

    private sealed record ReviewSeedDto(
        int Rating,
        string? Comment,
        string? Date,
        string? ReviewerName,
        string? ReviewerEmail);

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
