using DummyJson.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DummyJson.Persistence.Configurations;

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users", "identity");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.UserName).IsUnique();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PhoneNumber).HasMaxLength(30);
        builder.Property(u => u.Image).HasMaxLength(500);
        builder.Property(u => u.Gender).HasMaxLength(20);
        builder.Property(u => u.Role).HasMaxLength(50).HasDefaultValue("user");



        // IAuditable
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.CreatedBy).HasMaxLength(256);
        builder.Property(u => u.UpdatedBy).HasMaxLength(256);

        // ISoftDelete
        builder.Property(u => u.IsDeleted).HasDefaultValue(false);
        builder.Property(u => u.DeletedBy).HasMaxLength(256);

        // Ignore domain events (not persisted directly)
        builder.Ignore(u => u.DomainEvents);
        builder.Ignore(u => u.Version);
    }
}
