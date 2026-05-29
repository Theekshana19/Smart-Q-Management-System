using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQ.Domain.Entities;

namespace SmartQ.Infrastructure.Persistence.Configurations;

public class TokenConfiguration : IEntityTypeConfiguration<Token>
{
    public void Configure(EntityTypeBuilder<Token> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.TokenNo).HasMaxLength(20).IsRequired();
        builder.Property(t => t.TokenPrefix).HasMaxLength(10).IsRequired();
        builder.HasIndex(t => t.TokenNo).IsUnique().HasDatabaseName("IX_Tokens_TokenNo");
        builder.HasIndex(t => new { t.Status, t.CreatedAt }).HasDatabaseName("IX_Tokens_Status_CreatedAt");
        builder.HasIndex(t => new { t.ServiceId, t.Status, t.CreatedAt }).HasDatabaseName("IX_Tokens_ServiceId_Status_CreatedAt");
        builder.HasIndex(t => new { t.SubServiceId, t.Status, t.CreatedAt }).HasDatabaseName("IX_Tokens_SubServiceId_Status_CreatedAt");
        builder.HasIndex(t => new { t.CounterId, t.Status }).HasDatabaseName("IX_Tokens_CounterId_Status");
        builder.HasIndex(t => t.CreatedAt).HasDatabaseName("IX_Tokens_CreatedAt");

        builder.HasOne(t => t.Language).WithMany(l => l.Tokens).HasForeignKey(t => t.LanguageId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Service).WithMany(s => s.Tokens).HasForeignKey(t => t.ServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.SubService).WithMany(s => s.Tokens).HasForeignKey(t => t.SubServiceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(t => t.Counter).WithMany(c => c.Tokens).HasForeignKey(t => t.CounterId).OnDelete(DeleteBehavior.SetNull);
    }
}
