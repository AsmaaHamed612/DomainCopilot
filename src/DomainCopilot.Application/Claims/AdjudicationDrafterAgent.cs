using DomainCopilot.Application.Contracts;
using DomainCopilot.Domain.Enums;
using DomainCopilot.Domain.Services;
using DomainCopilot.Domain.ValueObjects;

namespace DomainCopilot.Application.Claims;

public sealed class AdjudicationDrafterAgent(ClaimPayoutCalculator calculator) : IAdjudicationDrafterAgent
{
    public ClaimAdjudicationResult Draft(
        ClaimAdjudicationRequest request,
        CoverageMatchResult coverage,
        ExclusionAnalysisResult exclusions)
    {
        var version = coverage.PolicyVersion?.Version;
        if (!coverage.IsMatched)
            return Refer(request, coverage.Summary, version);

        if (exclusions.RequiresHumanReview)
            return Refer(request, exclusions.Summary, version);

        try
        {
            var payout = calculator.Calculate(coverage.Coverage!, request.ClaimedAmount);
            var recommendation = payout.Amount == 0m ? RecommendationType.Reject : RecommendationType.Approve;
            var reason = payout.Amount == 0m
                ? "The covered amount does not exceed the applicable deductible. A human adjuster must approve the final decision."
                : "Coverage matched and the payout was calculated deterministically. A human adjuster must approve the final decision.";
            return new ClaimAdjudicationResult(recommendation, payout, reason, version);
        }
        catch (InvalidOperationException exception)
        {
            return Refer(request, $"The claim could not be safely calculated ({exception.Message}); an adjuster must review it.", version);
        }
    }

    private static ClaimAdjudicationResult Refer(ClaimAdjudicationRequest request, string reason, int? version) =>
        new(RecommendationType.ReferToHuman, Money.Zero(request.ClaimedAmount.Currency), reason, version);
}

