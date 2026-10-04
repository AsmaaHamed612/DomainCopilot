using DomainCopilot.Application.Contracts;
using DomainCopilot.Domain.Enums;
using DomainCopilot.Domain.ValueObjects;
using System.Diagnostics;

namespace DomainCopilot.Application.Claims;

public sealed class ClaimAdjudicationOrchestrator(
    ICoverageMatcherAgent coverageMatcher,
    IExclusionAnalystAgent exclusionAnalyst,
    IAdjudicationDrafterAgent drafter) : IClaimAdjudicationService
{
    public async Task<ClaimAdjudicationResult> AdjudicateAsync(
        ClaimAdjudicationRequest request,
        CancellationToken cancellationToken = default)
        => await AdjudicateAsync(request, static (_, _) => ValueTask.CompletedTask, cancellationToken);

    public async Task<ClaimAdjudicationResult> AdjudicateAsync(
        ClaimAdjudicationRequest request,
        Func<AgentProgressEvent, CancellationToken, ValueTask> onProgress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(onProgress);
        if (string.IsNullOrWhiteSpace(request.PolicyNumber))
            throw new ArgumentException("Policy number is required.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.CoverageCode))
            throw new ArgumentException("Coverage code is required.", nameof(request));

        cancellationToken.ThrowIfCancellationRequested();
        await onProgress(new AgentProgressEvent("Coverage Matcher", "agent", "started", "Matching the policy version and coverage."), cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var coverageStarted = Stopwatch.GetTimestamp();
        var coverage = await coverageMatcher.MatchAsync(request, cancellationToken);
        await onProgress(new AgentProgressEvent(
            "Coverage Matcher", "agent", coverage.IsMatched ? "completed" : "needs-review", coverage.Summary,
            (long)Stopwatch.GetElapsedTime(coverageStarted).TotalMilliseconds), cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        await onProgress(new AgentProgressEvent("Exclusion Analyst", "agent", "started", "Checking structured policy exclusions."), cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var exclusionStarted = Stopwatch.GetTimestamp();
        var exclusions = exclusionAnalyst.Analyze(request, coverage);
        await onProgress(new AgentProgressEvent(
            "Exclusion Analyst", "agent", exclusions.RequiresHumanReview ? "needs-review" : "completed", exclusions.Summary,
            (long)Stopwatch.GetElapsedTime(exclusionStarted).TotalMilliseconds), cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        await onProgress(new AgentProgressEvent("Adjudication Drafter", "agent", "started", "Drafting a recommendation."), cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var draftStarted = Stopwatch.GetTimestamp();
        var draft = drafter.Draft(request, coverage, exclusions);
        await onProgress(new AgentProgressEvent(
            "Adjudication Drafter", "agent", draft.Recommendation.ToString(), draft.Reason,
            (long)Stopwatch.GetElapsedTime(draftStarted).TotalMilliseconds), cancellationToken);

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
