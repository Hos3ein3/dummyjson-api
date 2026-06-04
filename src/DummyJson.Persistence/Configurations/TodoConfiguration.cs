using DummyJson.Domain.Todos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DummyJson.Persistence.Configurations;

public sealed class TodoConfiguration : IEntityTypeConfiguration<Todo>
{
    public void Configure(EntityTypeBuilder<Todo> builder)
    {
        builder.ToTable("Todos");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.UserId).IsRequired();
        builder.Property(t => t.TodoText).IsRequired().HasMaxLength(1000);
        builder.Property(t => t.Completed).HasDefaultValue(false);

        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.IsDeleted).HasDefaultValue(false);

        builder.HasIndex(t => t.UserId);

        builder.Ignore(t => t.DomainEvents);
        builder.Ignore(t => t.Version);
    }
}
