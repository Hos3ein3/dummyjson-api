using DummyJson.Domain.Products;
using DummyJson.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DummyJson.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Product"/> aggregate root.
/// Products are now stored in <b>PostgreSQL</b>.
///
/// <para>
/// <b>Category:</b> References <see cref="ProductCategory"/> via <c>CategoryId</c> FK.
/// </para>
/// <para>
/// <b>Images:</b> <c>jsonb</c> array column — simple string list, no join table needed.
/// </para>
/// <para>
/// <b>Tags:</b> Many-to-many via the existing <c>ProductTags</c> join table
/// (configured in <see cref="TagConfiguration"/>). No navigation on this side —
/// tags are queried via the join table directly.
/// </para>
/// <para>
/// <b>Reviews:</b> Stored in MongoDB (<c>product_reviews</c> collection).
/// No EF mapping here.
/// </para>
/// </summary>
public sealed class ProductEFConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        // ── Scalar columns ────────────────────────────────────────────────────

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Price)
            .HasColumnType("numeric(18,4)");

        builder.Property(p => p.DiscountPercentage)
            .HasColumnType("numeric(5,2)");

        builder.Property(p => p.Rating)
            .HasColumnType("numeric(4,2)");

        builder.Property(p => p.Brand)
            .HasMaxLength(200);

        builder.Property(p => p.Thumbnail)
            .HasMaxLength(1000);

        builder.Property(p => p.Sku)
            .HasMaxLength(100);

        builder.HasIndex(p => p.Sku)
            .IsUnique()
            .HasFilter("\"Sku\" <> ''")   // partial index — skip empty SKUs
            .HasDatabaseName("IX_Products_Sku");

        builder.Property(p => p.Barcode)
            .HasMaxLength(100);

        builder.Property(p => p.WarrantyInformation)
            .HasMaxLength(500);

        builder.Property(p => p.ShippingInformation)
            .HasMaxLength(500);

        builder.Property(p => p.AvailabilityStatus)
            .HasMaxLength(100)
            .HasDefaultValue("In Stock");

        builder.Property(p => p.ReturnPolicy)
            .HasMaxLength(500);



        // ── Category FK ───────────────────────────────────────────────────────
        builder.Property(p => p.CategoryId)
            .IsRequired();

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── IAuditable ────────────────────────────────────────────────────────
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(256);
        builder.Property(p => p.UpdatedBy).HasMaxLength(256);

        // ── ISoftDelete ───────────────────────────────────────────────────────
        builder.Property(p => p.IsDeleted).HasDefaultValue(false);
        builder.Property(p => p.DeletedBy).HasMaxLength(256);

        // ── IConcurrent ───────────────────────────────────────────────────────
        builder.Property(p => p.ConcurrencyStamp)
            .IsConcurrencyToken();

        // ── Ignored members ───────────────────────────────────────────────────
        builder.Ignore(p => p.DomainEvents);
        builder.Ignore(p => p.Version);
    }
}
