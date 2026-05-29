using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQ.Domain.Entities;

namespace SmartQ.Infrastructure.Persistence.Configurations;

public class DailyTokenSequenceConfiguration : IEntityTypeConfiguration<DailyTokenSequence>
{
    public void Configure(EntityTypeBuilder<DailyTokenSequence> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.TokenPrefix).HasMaxLength(10).IsRequired();
        builder.HasIndex(d => new { d.SequenceDate, d.SubServiceId, d.TokenPrefix }).IsUnique();
        builder.HasOne(d => d.SubService).WithMany(s => s.DailySequences).HasForeignKey(d => d.SubServiceId);
    }
}
