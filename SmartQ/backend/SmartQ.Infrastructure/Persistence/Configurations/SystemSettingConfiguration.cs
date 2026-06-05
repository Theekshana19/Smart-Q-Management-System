using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQ.Domain.Entities;

namespace SmartQ.Infrastructure.Persistence.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.SettingKey).HasMaxLength(100).IsRequired();
        builder.Property(s => s.SettingValue).HasMaxLength(2000).IsRequired();
        builder.Property(s => s.DataType).HasMaxLength(20).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.HasIndex(s => s.SettingKey).IsUnique();
    }
}
