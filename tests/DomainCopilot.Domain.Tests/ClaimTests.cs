using DomainCopilot.Domain.Entities;
using DomainCopilot.Domain.Enums;
using DomainCopilot.Domain.Services;
using DomainCopilot.Domain.ValueObjects;

namespace DomainCopilot.Domain.Tests;

public sealed class ClaimTests
{
    [Fact]
    public void NewClaim_StartsAsReceived()
    {
        var claim = new Claim("CLM-001", "POL-001", "WATER", new DateOnly(2026, 9, 1), new Money(1000, "EGP"), "Water damage");

        Assert.Equal(ClaimStatus.Received, claim.Status);
    }

    [Fact]
    public void Claim_CanMoveToHumanApproval()
    {
        var claim = new Claim("CLM-001", "POL-001", "WATER", new DateOnly(2026, 9, 1), new Money(1000, "EGP"), "Water damage");

        claim.MarkUnderReview();
        claim.MarkPendingHumanApproval();

        Assert.Equal(ClaimStatus.PendingHumanApproval, claim.Status);
    }

    [Fact]
    public void Payout_IsCalculatedOutsideTheLLM()
    {
        var coverage = new Coverage("WATER", "Water Damage", new Money(5000, "EGP"), new Money(500, "EGP"));
        var calculator = new ClaimPayoutCalculator();

        var result = calculator.Calculate(coverage, new Money(3000, "EGP"));

        Assert.Equal(2500m, result.Amount);
    }

    [Fact]
    public void Payout_IsCappedByCoverageLimit()
    {
        var coverage = new Coverage("WATER", "Water Damage", new Money(5000, "EGP"), new Money(500, "EGP"));
        var calculator = new ClaimPayoutCalculator();

        var result = calculator.Calculate(coverage, new Money(7000, "EGP"));

        Assert.Equal(4500m, result.Amount);
    }

    [Fact]
    public void Policy_SelectsVersionContainingLossDate()
    {
        var policy = new Policy("POL-001");
        policy.AddVersion(new PolicyVersion("POL-001", 1, new PolicyPeriod(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31))));
        policy.AddVersion(new PolicyVersion("POL-001", 2, new PolicyPeriod(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31))));

        var version = policy.FindVersion(new DateOnly(2026, 9, 1));

        Assert.NotNull(version);
        Assert.Equal(2, version!.Version);
    }
}
