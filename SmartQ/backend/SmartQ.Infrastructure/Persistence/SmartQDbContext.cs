using Microsoft.EntityFrameworkCore;
using SmartQ.Domain.Entities;
using SmartQ.Infrastructure.Persistence.Seed;

namespace SmartQ.Infrastructure.Persistence;

public class SmartQDbContext : DbContext
{
    public SmartQDbContext(DbContextOptions<SmartQDbContext> options) : base(options) { }

    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceTranslation> ServiceTranslations => Set<ServiceTranslation>();
    public DbSet<SubService> SubServices => Set<SubService>();
    public DbSet<SubServiceTranslation> SubServiceTranslations => Set<SubServiceTranslation>();
    public DbSet<Counter> Counters => Set<Counter>();
    public DbSet<CounterServiceAssignment> CounterServiceAssignments => Set<CounterServiceAssignment>();
    public DbSet<StaffUser> StaffUsers => Set<StaffUser>();
    public DbSet<Token> Tokens => Set<Token>();
    public DbSet<DailyTokenSequence> DailyTokenSequences => Set<DailyTokenSequence>();
    public DbSet<TokenStatusHistory> TokenStatusHistories => Set<TokenStatusHistory>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<VoiceTemplate> VoiceTemplates => Set<VoiceTemplate>();
    public DbSet<DisplayMessage> DisplayMessages => Set<DisplayMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartQDbContext).Assembly);
        SeedData.Apply(modelBuilder);
    }
}
