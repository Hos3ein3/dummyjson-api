using DummyJson.Domain.Comments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DummyJson.Persistence.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Body)
            .IsRequired()
            .HasMaxLength(2000);

        // PostId is nullable — seeded comments reference DummyJSON integer IDs
        // that cannot be correlated with our Guid-keyed Post collection
        builder.Property(c => c.PostId);

        builder.Property(c => c.Username)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Likes)
            .HasDefaultValue(0);

        // IAuditable
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(256);
        builder.Property(c => c.UpdatedBy).HasMaxLength(256);

        // ISoftDelete
        builder.Property(c => c.IsDeleted).HasDefaultValue(false);
        builder.Property(c => c.DeletedBy).HasMaxLength(256);

        // AggregateRoot members not persisted directly
        builder.Ignore(c => c.DomainEvents);
        builder.Ignore(c => c.Version);
    }
}
