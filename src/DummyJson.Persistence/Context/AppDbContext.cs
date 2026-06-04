using DummyJson.Domain.Carts;
using DummyJson.Domain.Comments;
using DummyJson.Domain.Common.Interfaces;
using DummyJson.Domain.Common.Primitives;
using DummyJson.Domain.Quotes;
using DummyJson.Domain.Recipes;
using DummyJson.Domain.Todos;
using DummyJson.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DummyJson.Persistence.Context;

/// <summary>
/// Main EF Core DbContext for relational databases (PostgreSQL primary, SQL Server secondary).
/// Extends <see cref="IdentityDbContext"/> to include ASP.NET Core Identity tables.
/// </summary>
public sealed class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── DbSets ─────────────────────────────────────────────────────────────
    public DbSet<User> DomainUsers { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Todo> Todos { get; set; }
    public DbSet<Quote> Quotes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<ApplicationUserRefreshToken> UserRefreshTokens { get; set; }

    // ── Model Configuration ───────────────────────────────────────────────────

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Rename ASP.NET Identity tables to a custom schema
        builder.Entity<ApplicationUser>().ToTable("Users", "identity");
        builder.Entity<ApplicationRole>().ToTable("Roles", "identity");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles", "identity");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", "identity");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", "identity");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", "identity");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", "identity");
        builder.Entity<ApplicationUserRefreshToken>().ToTable("UserRefreshTokens", "identity");

        // Apply all entity configurations from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Enforce Unique PhoneNumber
        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.PhoneNumber)
            .IsUnique();

        // Global soft-delete query filters
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var notDeleted = System.Linq.Expressions.Expression.Not(property);
                var lambda = System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);
                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    // ── Audit & Domain Events ─────────────────────────────────────────────────

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInfo()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(IAuditable.CreatedAt)).CurrentValue = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<IConcurrent>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            {
                entry.Property(nameof(IConcurrent.ConcurrencyStamp)).CurrentValue = DummyJson.Domain.Common.Primitives.IdGenerator.NewGuid();
            }
        }
    }

    /// <summary>
    /// Collects all domain events from tracked aggregates and clears them.
    /// Called by the DomainEventDispatcher after successful save.
    /// </summary>
    public IReadOnlyList<IDomainEvent> CollectDomainEvents()
    {
        var events = ChangeTracker
            .Entries<Entity<Guid>>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        foreach (var entry in ChangeTracker.Entries<Entity<Guid>>())
            entry.Entity.ClearDomainEvents();

        return events;
    }
}
