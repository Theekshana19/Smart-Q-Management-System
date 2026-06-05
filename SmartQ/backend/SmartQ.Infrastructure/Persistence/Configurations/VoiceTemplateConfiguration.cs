using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQ.Domain.Entities;

namespace SmartQ.Infrastructure.Persistence.Configurations;

public class VoiceTemplateConfiguration : IEntityTypeConfiguration<VoiceTemplate>
{
    public void Configure(EntityTypeBuilder<VoiceTemplate> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.EventType).HasMaxLength(50).IsRequired();
        builder.Property(v => v.TemplateText).HasMaxLength(1000).IsRequired();
        builder.HasIndex(v => new { v.LanguageId, v.EventType }).IsUnique();
        builder.HasIndex(v => new { v.LanguageId, v.IsActive });

        builder.HasOne(v => v.Language)
            .WithMany(l => l.VoiceTemplates)
            .HasForeignKey(v => v.LanguageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
