using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQ.Domain.Entities;

namespace SmartQ.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Code).HasMaxLength(20).IsRequired();
        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.Property(s => s.Icon).HasMaxLength(50);
        builder.HasIndex(s => s.Code).IsUnique();
        builder.HasIndex(s => new { s.IsActive, s.DisplayOrder });
    }
}
