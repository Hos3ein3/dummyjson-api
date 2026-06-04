using DummyJson.Domain.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DummyJson.Persistence.Configurations;

public sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.UserId).IsRequired();

        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey("CartId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.IsDeleted).HasDefaultValue(false);
        builder.Ignore(c => c.DomainEvents);
        builder.Ignore(c => c.Version);
        builder.Ignore(c => c.Total);
        builder.Ignore(c => c.DiscountedTotal);
        builder.Ignore(c => c.TotalProducts);
        builder.Ignore(c => c.TotalQuantity);
    }
}

public sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();
        builder.Property(i => i.ProductId).IsRequired();
        builder.Property(i => i.Title).IsRequired().HasMaxLength(300);
        builder.Property(i => i.Price).HasPrecision(18, 2);
        builder.Property(i => i.DiscountPercentage).HasPrecision(5, 2);
        builder.Property(i => i.Quantity).IsRequired();

        builder.Ignore(i => i.DomainEvents);
        builder.Ignore(i => i.Total);
        builder.Ignore(i => i.DiscountedTotal);
    }
}
