using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQ.Domain.Entities;

namespace SmartQ.Infrastructure.Persistence.Configurations;

public class SubServiceConfiguration : IEntityTypeConfiguration<SubService>
{
    public void Configure(EntityTypeBuilder<SubService> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Code).HasMaxLength(30).IsRequired();
        builder.Property(s => s.TokenPrefix).HasMaxLength(10).IsRequired();
        builder.HasIndex(s => new { s.ServiceId, s.Code }).IsUnique();
        builder.HasIndex(s => new { s.ServiceId, s.IsActive, s.DisplayOrder });
        builder.HasIndex(s => s.TokenPrefix);
        builder.HasOne(s => s.Service).WithMany(sv => sv.SubServices).HasForeignKey(s => s.ServiceId);
    }
}
