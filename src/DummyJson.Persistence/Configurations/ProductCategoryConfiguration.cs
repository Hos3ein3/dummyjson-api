using DummyJson.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DummyJson.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="ProductCategory"/> lookup table.
///
/// Category is extracted from the inline <c>Category</c> string on Product into its own
/// normalised table so that category names can be renamed, hierarchies can be introduced,
/// and category-level metadata (description, image URL, etc.) can be added without
/// touching every product row.
/// </summary>
public sealed class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(100);

        // Slug must be unique — it is the stable identifier used by product rows
        builder.HasIndex(c => c.Slug)
            .IsUnique()
            .HasDatabaseName("IX_ProductCategories_Slug");

        // Name is also unique (two categories cannot have the same display name)
        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasDatabaseName("IX_ProductCategories_Name");

        // Ignore domain events from Entity<Guid>
        builder.Ignore(c => c.DomainEvents);
    }
}
