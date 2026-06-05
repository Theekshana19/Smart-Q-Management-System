using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQ.Domain.Entities;

namespace SmartQ.Infrastructure.Persistence.Configurations;

public class StaffUserConfiguration : IEntityTypeConfiguration<StaffUser>
{
    public void Configure(EntityTypeBuilder<StaffUser> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Username).HasMaxLength(100).IsRequired();
        builder.Property(s => s.FullName).HasMaxLength(150).IsRequired();
        builder.Property(s => s.Email).HasMaxLength(200).IsRequired();
        builder.HasIndex(s => s.Username).IsUnique();
        builder.HasIndex(s => s.Email).IsUnique();
        builder.HasIndex(s => s.CounterId);
    }
}
