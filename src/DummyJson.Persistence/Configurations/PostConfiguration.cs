using DummyJson.Domain.Posts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DummyJson.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Post"/> aggregate root.
/// Posts are now stored in <b>PostgreSQL</b>.
///
/// <para>
/// <b>Tags:</b> Stored as a denormalised <c>jsonb</c> array column for fast read queries.
/// The <c>PostTags</c> join table (configured in <see cref="TagConfiguration"/>)
/// now has a proper <c>PostId → Posts.Id</c> FK since Post is relational.
/// </para>
/// </summary>
public sealed class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("Posts");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        // ── Scalar columns ────────────────────────────────────────────────────

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.Body)
            .IsRequired();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("IX_Posts_UserId");

        // ── Tags (denormalised jsonb array) ────────────────────────────────────
        builder.Property(p => p.Tags)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        // ── Counters ──────────────────────────────────────────────────────────
        builder.Property(p => p.Views).HasDefaultValue(0);
        builder.Property(p => p.Likes).HasDefaultValue(0);
        builder.Property(p => p.Dislikes).HasDefaultValue(0);

        // ── IAuditable ────────────────────────────────────────────────────────
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(256);
        builder.Property(p => p.UpdatedBy).HasMaxLength(256);

        // ── ISoftDelete ───────────────────────────────────────────────────────
        builder.Property(p => p.IsDeleted).HasDefaultValue(false);
        builder.Property(p => p.DeletedBy).HasMaxLength(256);

        // ── Ignored members ───────────────────────────────────────────────────
        builder.Ignore(p => p.DomainEvents);
        builder.Ignore(p => p.Version);
    }
}
