using DomainCopilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Infrastructure.Persistence;

public sealed class DomainCopilotDbContext(DbContextOptions<DomainCopilotDbContext> options) : DbContext(options)
{
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<PolicyVersion> PolicyVersions => Set<PolicyVersion>();
    public DbSet<ReviewQueueItem> ReviewQueueItems => Set<ReviewQueueItem>();
    public DbSet<ReviewDecisionAudit> ReviewDecisionAudits => Set<ReviewDecisionAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(claim => claim.Id);
            entity.HasIndex(claim => claim.ClaimNumber).IsUnique();
            entity.Property(claim => claim.ClaimNumber).HasMaxLength(64).IsRequired();
            entity.Property(claim => claim.PolicyNumber).HasMaxLength(64).IsRequired();
            entity.Property(claim => claim.CoverageCode).HasMaxLength(64).IsRequired();
            entity.Property(claim => claim.Description).IsRequired();
            entity.Property(claim => claim.Status).HasConversion<int>();
            entity.OwnsOne(claim => claim.ClaimedAmount, money =>
            {
                money.Property(value => value.Amount).HasColumnName("ClaimedAmount").HasPrecision(18, 2);
                money.Property(value => value.Currency).HasColumnName("Currency").HasMaxLength(3).IsRequired();
            });
        });

        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(policy => policy.PolicyNumber);
            entity.Property(policy => policy.PolicyNumber).HasMaxLength(64);
            entity.HasMany(policy => policy.Versions).WithOne().HasForeignKey(version => version.PolicyNumber).OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(policy => policy.Versions).HasField("_versions").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<PolicyVersion>(entity =>
        {
            entity.HasKey(version => new { version.PolicyNumber, version.Version });
            entity.Property(version => version.PolicyNumber).HasMaxLength(64);
            entity.OwnsOne(version => version.Period, period =>
            {
                period.Property(value => value.EffectiveFrom).HasColumnName("EffectiveFrom").HasColumnType("date");
                period.Property(value => value.EffectiveTo).HasColumnName("EffectiveTo").HasColumnType("date");
            });
            entity.HasMany(version => version.Coverages).WithOne().HasForeignKey("PolicyNumber", "PolicyVersion").OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(version => version.Coverages).HasField("_coverages").UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.HasMany(version => version.Exclusions).WithOne().HasForeignKey("PolicyNumber", "PolicyVersion").OnDelete(DeleteBehavior.Cascade);
            entity.Navigation(version => version.Exclusions).HasField("_exclusions").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Coverage>(entity =>
        {
            entity.ToTable("Coverages");
            entity.HasKey("PolicyNumber", "PolicyVersion", nameof(Coverage.Code));
            entity.Property(coverage => coverage.Code).HasMaxLength(64);
            entity.Property(coverage => coverage.Name).HasMaxLength(256);
            entity.OwnsOne(coverage => coverage.Limit, money =>
            {
                money.Property(value => value.Amount).HasColumnName("LimitAmount").HasPrecision(18, 2);
                money.Property(value => value.Currency).HasColumnName("LimitCurrency").HasMaxLength(3).IsRequired();
            });
            entity.OwnsOne(coverage => coverage.Deductible, money =>
            {
                money.Property(value => value.Amount).HasColumnName("DeductibleAmount").HasPrecision(18, 2);
                money.Property(value => value.Currency).HasColumnName("DeductibleCurrency").HasMaxLength(3).IsRequired();
            });
        });

        modelBuilder.Entity<Exclusion>(entity =>
        {
            entity.ToTable("Exclusions");
            entity.HasKey("PolicyNumber", "PolicyVersion", nameof(Exclusion.Code));
            entity.Property(exclusion => exclusion.Code).HasMaxLength(64);
            entity.Property(exclusion => exclusion.Description).IsRequired();
        });

        modelBuilder.Entity<ReviewQueueItem>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.AssignedReviewerId).HasMaxLength(128);
            entity.Property(item => item.Priority).HasConversion<int>();
            entity.Property(item => item.Status).HasConversion<int>();
            entity.Property(item => item.Decision).HasMaxLength(2048);
            entity.Property(item => item.ReviewerComment).HasMaxLength(2048);
            entity.HasOne<Claim>().WithMany().HasForeignKey(item => item.ClaimId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReviewDecisionAudit>(entity =>
        {
            entity.HasKey(audit => audit.Id);
            entity.Property(audit => audit.ReviewerId).HasMaxLength(128).IsRequired();
            entity.Property(audit => audit.Action).HasMaxLength(32).IsRequired();
            entity.Property(audit => audit.Decision).HasMaxLength(2048).IsRequired();
            entity.Property(audit => audit.Comment).HasMaxLength(2048).IsRequired();
            entity.HasIndex(audit => new { audit.ReviewQueueItemId, audit.CreatedAt });
            entity.HasOne<ReviewQueueItem>().WithMany().HasForeignKey(audit => audit.ReviewQueueItemId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
