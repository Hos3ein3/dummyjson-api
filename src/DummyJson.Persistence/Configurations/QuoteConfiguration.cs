using DummyJson.Domain.Quotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DummyJson.Persistence.Configurations;

public sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes");
        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .ValueGeneratedNever();

        builder.Property(q => q.QuoteText)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(q => q.Author)
            .IsRequired()
            .HasMaxLength(200);

        // IAuditable
        builder.Property(q => q.CreatedAt).IsRequired();
        builder.Property(q => q.CreatedBy).HasMaxLength(256);
        builder.Property(q => q.UpdatedBy).HasMaxLength(256);

        // ISoftDelete
        builder.Property(q => q.IsDeleted).HasDefaultValue(false);
        builder.Property(q => q.DeletedBy).HasMaxLength(256);

        // AggregateRoot members not persisted directly
        builder.Ignore(q => q.DomainEvents);
        builder.Ignore(q => q.Version);
    }
}
