using System.Text.Json;
using DomainCopilot.Application.Claims;
using DomainCopilot.Application.Contracts;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Domain.Enums;
using DomainCopilot.Domain.Services;
using DomainCopilot.Domain.ValueObjects;

namespace DomainCopilot.Application.Tests;

public sealed class ClaimAdjudicationEvaluationTests
{
    [Fact]
    public async Task GoldenSet_ProducesExpectedDeterministicRecommendations()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "evaluation", "claims-golden-set.json");
        await using var file = File.OpenRead(path);
        var goldenSet = await JsonSerializer.DeserializeAsync<GoldenSet>(file, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(goldenSet);
        Assert.Equal(25, goldenSet.Cases.Count);
        Assert.Equal(5, goldenSet.Cases.Count(item => item.Adversarial));
        var correct = 0;

        foreach (var item in goldenSet.Cases)
        {
            var policy = item.PolicyNumber == "POL-DEMO-001" ? CreatePolicy(item.ExclusionText) : null;
            var service = CreateOrchestrator(policy);
            var result = await service.AdjudicateAsync(new ClaimAdjudicationRequest(
                item.PolicyNumber,
                DateOnly.Parse(item.LossDate),
                item.CoverageCode,
                new Money(item.ClaimedAmount, item.Currency)));

            Assert.Equal(item.ExpectedRecommendation, result.Recommendation.ToString());
            Assert.Equal(item.ExpectedPayout, result.CalculatedPayout.Amount);
            correct++;
        }

        Assert.Equal(25, correct);
    }

    private static Policy CreatePolicy(string? exclusionText)
    {
        var policy = new Policy("POL-DEMO-001");
        var first = new PolicyVersion("POL-DEMO-001", 1,
            new PolicyPeriod(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31)));
        first.AddCoverage(new Coverage("WATER", "Water damage", new Money(100000m, "EGP"), new Money(5000m, "EGP")));
        policy.AddVersion(first);

        var second = new PolicyVersion("POL-DEMO-001", 2,
            new PolicyPeriod(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));
        second.AddCoverage(new Coverage("WATER", "Water damage", new Money(250000m, "EGP"), new Money(10000m, "EGP")));
        second.AddExclusion(new Exclusion("FLOOD", exclusionText ?? "Flood damage requires adjuster review."));
        policy.AddVersion(second);
        return policy;
    }

    private static ClaimAdjudicationOrchestrator CreateOrchestrator(Policy? policy) => new(
        new CoverageMatcherAgent(new FakePolicyRepository(policy)),
        new ExclusionAnalystAgent(),
        new AdjudicationDrafterAgent(new ClaimPayoutCalculator()));

    private sealed class FakePolicyRepository(Policy? policy) : IPolicyRepository
    {
        public Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default) =>
            Task.FromResult(policy is not null && policy.PolicyNumber == policyNumber ? policy : null);
    }

    private sealed record GoldenSet(IReadOnlyList<GoldenCase> Cases);

    private sealed record GoldenCase(
        string Id,
        string PolicyNumber,
        string LossDate,
        string CoverageCode,
        decimal ClaimedAmount,
        string Currency,
        string ExpectedRecommendation,
        decimal ExpectedPayout,
        bool Adversarial = false,
        string? ExclusionText = null);
}
