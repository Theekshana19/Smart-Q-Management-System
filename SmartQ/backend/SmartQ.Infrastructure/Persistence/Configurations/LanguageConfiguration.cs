using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQ.Domain.Entities;

namespace SmartQ.Infrastructure.Persistence.Configurations;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Code).HasMaxLength(10).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(100).IsRequired();
        builder.Property(l => l.NativeName).HasMaxLength(100).IsRequired();
        builder.HasIndex(l => l.Code).IsUnique();
        builder.HasIndex(l => new { l.IsActive, l.DisplayOrder });
    }
}
