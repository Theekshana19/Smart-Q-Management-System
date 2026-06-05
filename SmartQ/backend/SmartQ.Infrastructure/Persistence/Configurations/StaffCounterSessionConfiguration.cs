using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQ.Domain.Entities;
using SmartQ.Domain.Enums;

namespace SmartQ.Infrastructure.Persistence.Configurations;

public class StaffCounterSessionConfiguration : IEntityTypeConfiguration<StaffCounterSession>
{
    public void Configure(EntityTypeBuilder<StaffCounterSession> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.LoginIp).HasMaxLength(64);
        builder.Property(s => s.DeviceName).HasMaxLength(200);
        builder.Property(s => s.Remarks).HasMaxLength(500);

        builder.HasIndex(s => new { s.StaffUserId, s.Status });
        builder.HasIndex(s => new { s.CounterId, s.Status });
        builder.HasIndex(s => s.StartedAt);
        builder.HasIndex(s => s.EndedAt);

        builder.HasOne(s => s.StaffUser)
            .WithMany(u => u.CounterSessions)
            .HasForeignKey(s => s.StaffUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Counter)
            .WithMany()
            .HasForeignKey(s => s.CounterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
