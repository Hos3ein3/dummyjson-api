using System.Text.Json;
using System.Text.Json.Serialization;
using DummyJson.Domain.Carts;
using DummyJson.Domain.Posts;
using DummyJson.Domain.Products;
using DummyJson.Domain.Todos;
using DummyJson.Domain.Users;
using DummyJson.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace DummyJson.Persistence.Seeding;

/// <summary>
/// Seeds the databases from local JSON files (SeedData/*.json) on first run.
/// Only seeds if tables / collections are empty.
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
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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

    public async Task SeedAsync(string seedDataPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting database seeding from {Path}", seedDataPath);

        await _dbContext.Database.MigrateAsync(cancellationToken);

        await SeedIdentityAsync(cancellationToken);
        await SeedUsersAsync(seedDataPath, cancellationToken);
        await SeedProductsAsync(seedDataPath, cancellationToken);
        await SeedPostsAsync(seedDataPath, cancellationToken);
        await SeedTodosAsync(seedDataPath, cancellationToken);
        await SeedCartsAsync(seedDataPath, cancellationToken);

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
            {
                await _roleManager.CreateAsync(role);
            }
        }

        if (await _userManager.FindByEmailAsync("developer@dummy.com") == null)
        {
            var dev = new ApplicationUser { UserName = "developer", Email = "developer@dummy.com", FirstName = "Dev", DomainUserId = Guid.CreateVersion7() };
            await _userManager.CreateAsync(dev, "Dev@1234");
            await _userManager.AddToRoleAsync(dev, "Developer");
        }

        if (await _userManager.FindByEmailAsync("admin@dummy.com") == null)
        {
            var admin = new ApplicationUser { UserName = "admin", Email = "admin@dummy.com", FirstName = "Admin", DomainUserId = Guid.CreateVersion7() };
            await _userManager.CreateAsync(admin, "Admin@123");
            await _userManager.AddToRoleAsync(admin, "Admin");
        }

        _logger.LogInformation("Identity roles and users seeded.");
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    private async Task SeedUsersAsync(string path, CancellationToken ct)
    {
        if (await _dbContext.DomainUsers.AnyAsync(ct))
        {
            _logger.LogInformation("Users already seeded, skipping.");
            return;
        }

        var file = Path.Combine(path, "users.json");
        if (!File.Exists(file)) { _logger.LogWarning("users.json not found, skipping."); return; }

        var json = await File.ReadAllTextAsync(file, ct);
        var root = JsonSerializer.Deserialize<SeedRoot<UserSeedDto>>(json, _jsonOptions);
        if (root?.Items is null) return;

        foreach (var dto in root.Items)
        {
            var result = User.Create(dto.FirstName, dto.LastName, dto.Username, dto.Email,
                dto.Phone ?? "", dto.Image, dto.Gender, dto.BirthDate);
            if (result.IsSuccess)
                await _dbContext.DomainUsers.AddAsync(result.Value, ct);
        }

        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} users.", root.Items.Count);
    }

    // ── Products (MongoDB) ────────────────────────────────────────────────────

    private async Task SeedProductsAsync(string path, CancellationToken ct)
    {
        var count = await _mongoContext.Products.CountDocumentsAsync(FilterDefinition<Product>.Empty, null, ct);
        if (count > 0) { _logger.LogInformation("Products already seeded, skipping."); return; }

        var file = Path.Combine(path, "products.json");
        if (!File.Exists(file)) { _logger.LogWarning("products.json not found, skipping."); return; }

        var json = await File.ReadAllTextAsync(file, ct);
        var root = JsonSerializer.Deserialize<SeedRoot<ProductSeedDto>>(json, _jsonOptions);
        if (root?.Items is null) return;

        var products = new List<Product>();
        foreach (var dto in root.Items)
        {
            var result = Product.Create(
                dto.Title, dto.Description, dto.Price, dto.DiscountPercentage,
                dto.Stock, dto.Brand ?? "", dto.Category, dto.Thumbnail,
                dto.Images ?? [], dto.Tags ?? [], dto.Sku ?? "",
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
        var count = await _mongoContext.Posts.CountDocumentsAsync(FilterDefinition<Post>.Empty, null, ct);
        if (count > 0) { _logger.LogInformation("Posts already seeded, skipping."); return; }

        var file = Path.Combine(path, "posts.json");
        if (!File.Exists(file)) { _logger.LogWarning("posts.json not found, skipping."); return; }

        var json = await File.ReadAllTextAsync(file, ct);
        var root = JsonSerializer.Deserialize<SeedRoot<PostSeedDto>>(json, _jsonOptions);
        if (root?.Items is null) return;

        var posts = new List<Post>();
        foreach (var dto in root.Items)
        {
            var result = Post.Create(Guid.CreateVersion7(), dto.Title, dto.Body, dto.Tags ?? []);
            if (result.IsSuccess) posts.Add(result.Value);
        }

        if (posts.Count > 0)
            await _mongoContext.Posts.InsertManyAsync(posts, null, ct);

        _logger.LogInformation("Seeded {Count} posts.", posts.Count);
    }

    // ── Todos ─────────────────────────────────────────────────────────────────

    private async Task SeedTodosAsync(string path, CancellationToken ct)
    {
        if (await _dbContext.Todos.AnyAsync(ct)) return;

        var file = Path.Combine(path, "todos.json");
        if (!File.Exists(file)) { _logger.LogWarning("todos.json not found, skipping."); return; }

        var json = await File.ReadAllTextAsync(file, ct);
        var root = JsonSerializer.Deserialize<SeedRoot<TodoSeedDto>>(json, _jsonOptions);
        if (root?.Items is null) return;

        foreach (var dto in root.Items)
        {
            var result = Todo.Create(Guid.CreateVersion7(), dto.Todo);
            if (result.IsSuccess)
            {
                if (dto.Completed) result.Value.Complete();
                await _dbContext.Todos.AddAsync(result.Value, ct);
            }
        }

        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} todos.", root.Items.Count);
    }

    // ── Carts ─────────────────────────────────────────────────────────────────

    private async Task SeedCartsAsync(string path, CancellationToken ct)
    {
        if (await _dbContext.Carts.AnyAsync(ct)) return;

        var file = Path.Combine(path, "carts.json");
        if (!File.Exists(file)) { _logger.LogWarning("carts.json not found, skipping."); return; }

        var json = await File.ReadAllTextAsync(file, ct);
        var root = JsonSerializer.Deserialize<SeedRoot<CartSeedDto>>(json, _jsonOptions);
        if (root?.Items is null) return;

        foreach (var dto in root.Items)
        {
            var cart = Cart.Create(Guid.CreateVersion7());
            foreach (var item in dto.Products ?? [])
                cart.AddItem(Guid.CreateVersion7(), item.Title ?? "", item.Price, item.DiscountPercentage, item.Quantity);
            await _dbContext.Carts.AddAsync(cart, ct);
        }

        await _dbContext.SaveChangesAsync(ct);
        _logger.LogInformation("Seeded {Count} carts.", root.Items.Count);
    }

    // ── Seed DTOs ──────────────────────────────────────────────────────────────

    private sealed class SeedRoot<T>
    {
        [JsonPropertyName("users")] public List<T>? Users { get; set; }
        [JsonPropertyName("products")] public List<T>? Products { get; set; }
        [JsonPropertyName("posts")] public List<T>? Posts { get; set; }
        [JsonPropertyName("todos")] public List<T>? Todos { get; set; }
        [JsonPropertyName("carts")] public List<T>? Carts { get; set; }

        public List<T>? Items => Users ?? Products ?? Posts ?? Todos ?? Carts;
    }

    private sealed record UserSeedDto(
        string FirstName, string LastName, string Username, string Email,
        string? Phone, string? Image, string? Gender, DateOnly? BirthDate);

    private sealed record ProductSeedDto(
        string Title, string Description, decimal Price, decimal DiscountPercentage,
        int Stock, string? Brand, string Category, string Thumbnail,
        List<string>? Images, List<string>? Tags, string? Sku, string? WarrantyInformation,
        string? ShippingInformation, string? AvailabilityStatus, string? ReturnPolicy,
        int MinimumOrderQuantity, ProductMetaSeedDto? Meta);

    private sealed record ProductMetaSeedDto(string? Barcode);

    private sealed record PostSeedDto(string Title, string Body, List<string>? Tags);

    private sealed record TodoSeedDto(string Todo, bool Completed);

    private sealed record CartSeedDto(List<CartProductSeedDto>? Products);

    private sealed record CartProductSeedDto(
        string? Title, decimal Price, decimal DiscountPercentage, int Quantity);
}
