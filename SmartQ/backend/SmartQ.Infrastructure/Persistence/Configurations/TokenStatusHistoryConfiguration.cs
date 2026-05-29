using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQ.Domain.Entities;

namespace SmartQ.Infrastructure.Persistence.Configurations;

public class TokenStatusHistoryConfiguration : IEntityTypeConfiguration<TokenStatusHistory>
{
    public void Configure(EntityTypeBuilder<TokenStatusHistory> builder)
    {
        builder.HasKey(h => h.Id);
        builder.HasIndex(h => new { h.TokenId, h.ChangedAt });
        builder.HasOne(h => h.Token).WithMany(t => t.StatusHistory).HasForeignKey(h => h.TokenId);
    }
}
