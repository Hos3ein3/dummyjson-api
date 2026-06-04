using DummyJson.Domain.Recipes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DummyJson.Persistence.Configurations;

public sealed class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("Recipes");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(r => r.Difficulty)
            .HasMaxLength(50);

        builder.Property(r => r.Cuisine)
            .HasMaxLength(100);

        builder.Property(r => r.Image)
            .HasMaxLength(500);

        builder.Property(r => r.Rating)
            .HasPrecision(4, 2);

        // List<string> columns — stored as JSON (PostgreSQL jsonb / SQL Server nvarchar)
        // This avoids extra join tables for simple string lists.
        builder.Property(r => r.Ingredients)
            .HasColumnType("jsonb");

        builder.Property(r => r.Instructions)
            .HasColumnType("jsonb");

        builder.Property(r => r.Tags)
            .HasColumnType("jsonb");

        builder.Property(r => r.MealType)
            .HasColumnType("jsonb");

        // IAuditable
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.CreatedBy).HasMaxLength(256);
        builder.Property(r => r.UpdatedBy).HasMaxLength(256);

        // ISoftDelete
        builder.Property(r => r.IsDeleted).HasDefaultValue(false);
        builder.Property(r => r.DeletedBy).HasMaxLength(256);

        // AggregateRoot members not persisted directly
        builder.Ignore(r => r.DomainEvents);
        builder.Ignore(r => r.Version);
    }
}
