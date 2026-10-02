using DomainCopilot.Application.Contracts;
using DomainCopilot.Domain.Enums;
using DomainCopilot.Domain.Services;
using DomainCopilot.Domain.ValueObjects;

namespace DomainCopilot.Application.Claims;

public sealed class ClaimAdjudicationService : IClaimAdjudicationService
{
    private readonly IPolicyRepository _policies;
    private readonly ClaimPayoutCalculator _calculator;

    public ClaimAdjudicationService(IPolicyRepository policies, ClaimPayoutCalculator calculator)
    {
        _policies = policies ?? throw new ArgumentNullException(nameof(policies));
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    public async Task<ClaimAdjudicationResult> AdjudicateAsync(
        ClaimAdjudicationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.PolicyNumber))
            throw new ArgumentException("Policy number is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.CoverageCode))
            throw new ArgumentException("Coverage code is required.", nameof(request));

        var policy = await _policies.GetByPolicyNumberAsync(request.PolicyNumber.Trim(), cancellationToken);
        if (policy is null)
            return ReferToHuman(request, "No matching policy was found; an adjuster must verify the policy.");

        var version = policy.FindVersion(request.LossDate);
        if (version is null)
            return ReferToHuman(request, "No policy version covers the loss date; an adjuster must resolve the gap.");

        var coverage = version.Coverages.FirstOrDefault(item =>
            string.Equals(item.Code, request.CoverageCode.Trim(), StringComparison.OrdinalIgnoreCase));
        if (coverage is null)
            return ReferToHuman(request, "The requested coverage is not listed in the applicable policy version.", version.Version);

        if (version.Exclusions.Count > 0)
            return ReferToHuman(request, "The applicable policy contains exclusions that require evidence review.", version.Version);

        var payout = _calculator.Calculate(coverage, request.ClaimedAmount);
        var recommendation = payout.Amount == 0m ? RecommendationType.Reject : RecommendationType.Approve;
        var reason = payout.Amount == 0m
            ? "The covered amount does not exceed the applicable deductible. A human adjuster must approve the final decision."
            : "Coverage matched and the payout was calculated deterministically. A human adjuster must approve the final decision.";

        return new ClaimAdjudicationResult(recommendation, payout, reason, version.Version);
    }

    private static ClaimAdjudicationResult ReferToHuman(ClaimAdjudicationRequest request, string reason, int? version = null) =>
        new(RecommendationType.ReferToHuman, Money.Zero(request.ClaimedAmount.Currency), reason, version);
}
