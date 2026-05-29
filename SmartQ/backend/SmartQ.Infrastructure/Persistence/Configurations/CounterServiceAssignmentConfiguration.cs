using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQ.Domain.Entities;

namespace SmartQ.Infrastructure.Persistence.Configurations;

public class CounterServiceAssignmentConfiguration : IEntityTypeConfiguration<CounterServiceAssignment>
{
    public void Configure(EntityTypeBuilder<CounterServiceAssignment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => new { a.CounterId, a.IsActive });
        builder.HasIndex(a => new { a.ServiceId, a.IsActive });
        builder.HasOne(a => a.Counter).WithMany(c => c.ServiceAssignments).HasForeignKey(a => a.CounterId);
        builder.HasOne(a => a.Service).WithMany(s => s.CounterAssignments).HasForeignKey(a => a.ServiceId);
    }
}
