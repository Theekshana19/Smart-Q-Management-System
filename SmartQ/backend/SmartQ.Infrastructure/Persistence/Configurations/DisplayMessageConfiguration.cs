using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQ.Domain.Entities;

namespace SmartQ.Infrastructure.Persistence.Configurations;

public class DisplayMessageConfiguration : IEntityTypeConfiguration<DisplayMessage>
{
    public void Configure(EntityTypeBuilder<DisplayMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.MessageKey).HasMaxLength(100).IsRequired();
        builder.Property(m => m.MessageText).HasMaxLength(2000).IsRequired();
        builder.HasIndex(m => new { m.MessageKey, m.LanguageId }).IsUnique();
        builder.HasIndex(m => new { m.LanguageId, m.IsActive });
        builder.HasIndex(m => m.MessageKey);

        builder.HasOne(m => m.Language)
            .WithMany(l => l.DisplayMessages)
            .HasForeignKey(m => m.LanguageId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
