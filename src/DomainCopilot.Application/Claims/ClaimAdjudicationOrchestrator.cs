using DomainCopilot.Application.Contracts;
using DomainCopilot.Domain.Enums;
using DomainCopilot.Domain.ValueObjects;

namespace DomainCopilot.Application.Claims;

public sealed class ClaimAdjudicationOrchestrator(
    ICoverageMatcherAgent coverageMatcher,
    IExclusionAnalystAgent exclusionAnalyst,
    IAdjudicationDrafterAgent drafter) : IClaimAdjudicationService
{
    public async Task<ClaimAdjudicationResult> AdjudicateAsync(
        ClaimAdjudicationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.PolicyNumber))
            throw new ArgumentException("Policy number is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.CoverageCode))
            throw new ArgumentException("Coverage code is required.", nameof(request));

        cancellationToken.ThrowIfCancellationRequested();
        var coverage = await coverageMatcher.MatchAsync(request, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var exclusions = exclusionAnalyst.Analyze(request, coverage);
        cancellationToken.ThrowIfCancellationRequested();
        var draft = drafter.Draft(request, coverage, exclusions);

        return draft with
        {
            AgentSteps =
            [
                new AgentStepResult("Coverage Matcher", coverage.IsMatched ? "completed" : "needs-review", coverage.Summary),
                new AgentStepResult("Exclusion Analyst", exclusions.RequiresHumanReview ? "needs-review" : "completed", exclusions.Summary),
                new AgentStepResult("Adjudication Drafter", draft.Recommendation.ToString(), draft.Reason)
            ]
        };
    }
}

