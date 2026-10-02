using DomainCopilot.Application.Claims;
using DomainCopilot.Application.Contracts;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Domain.Enums;
using DomainCopilot.Domain.Services;
using DomainCopilot.Domain.ValueObjects;

namespace DomainCopilot.Application.Tests;

public sealed class ClaimAdjudicationServiceTests
{
    [Fact]
    public async Task AdjudicateAsync_SelectsApplicableVersionAndCalculatesPayoutDeterministically()
    {
        var policy = CreatePolicy(includeExclusion: false);
        var service = CreateOrchestrator(policy);

        var result = await service.AdjudicateAsync(new ClaimAdjudicationRequest(
            "POL-001", new DateOnly(2025, 6, 15), "WATER", new Money(42000m, "EGP")));

        Assert.Equal(RecommendationType.Approve, result.Recommendation);
        Assert.Equal(37000m, result.CalculatedPayout.Amount);
        Assert.Equal(1, result.PolicyVersion);
    }

    [Fact]
    public async Task AdjudicateAsync_RefersToHumanWhenNoPolicyMatches()
    {
        var service = CreateOrchestrator(null);

        var result = await service.AdjudicateAsync(new ClaimAdjudicationRequest(
            "UNKNOWN", new DateOnly(2025, 6, 15), "WATER", new Money(42000m, "EGP")));

        Assert.Equal(RecommendationType.ReferToHuman, result.Recommendation);
        Assert.Equal(Money.Zero("EGP"), result.CalculatedPayout);
        Assert.Null(result.PolicyVersion);
    }

    [Fact]
    public async Task AdjudicateAsync_RefersToHumanWhenApplicableVersionHasExclusions()
    {
        var policy = CreatePolicy(includeExclusion: true);
        var service = CreateOrchestrator(policy);

        var result = await service.AdjudicateAsync(new ClaimAdjudicationRequest(
            "POL-001", new DateOnly(2026, 6, 15), "WATER", new Money(42000m, "EGP")));

        Assert.Equal(RecommendationType.ReferToHuman, result.Recommendation);
        Assert.Equal(2, result.PolicyVersion);
    }

    [Fact]
    public async Task AdjudicateAsync_RefersToHumanWhenLossDateFallsBetweenVersions()
    {
        var policy = CreatePolicy(includeExclusion: false);
        var service = CreateOrchestrator(policy);

        var result = await service.AdjudicateAsync(new ClaimAdjudicationRequest(
            "POL-001", new DateOnly(2024, 12, 31), "WATER", new Money(42000m, "EGP")));

        Assert.Equal(RecommendationType.ReferToHuman, result.Recommendation);
        Assert.Null(result.PolicyVersion);
    }

    private static Policy CreatePolicy(bool includeExclusion)
    {
        var policy = new Policy("POL-001");
        var first = new PolicyVersion("POL-001", 1, new PolicyPeriod(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31)));
        first.AddCoverage(new Coverage("WATER", "Water damage", new Money(100000m, "EGP"), new Money(5000m, "EGP")));
        policy.AddVersion(first);

        var second = new PolicyVersion("POL-001", 2, new PolicyPeriod(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));
        second.AddCoverage(new Coverage("WATER", "Water damage", new Money(250000m, "EGP"), new Money(10000m, "EGP")));
        if (includeExclusion) second.AddExclusion(new Exclusion("FLOOD", "Flood claims require review."));
        policy.AddVersion(second);
        return policy;
    }

    private static ClaimAdjudicationOrchestrator CreateOrchestrator(Policy? policy)
    {
        var repository = new FakePolicyRepository(policy);
        return new ClaimAdjudicationOrchestrator(
            new CoverageMatcherAgent(repository),
            new ExclusionAnalystAgent(),
            new AdjudicationDrafterAgent(new ClaimPayoutCalculator()));
    }

    private sealed class FakePolicyRepository(Policy? policy) : IPolicyRepository
    {
        public Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default) =>
            Task.FromResult(policy is not null && policy.PolicyNumber == policyNumber ? policy : null);
    }
}

