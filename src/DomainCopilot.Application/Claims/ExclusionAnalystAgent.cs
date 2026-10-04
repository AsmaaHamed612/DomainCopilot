using DomainCopilot.Application.Contracts;

namespace DomainCopilot.Application.Claims;

public sealed class ExclusionAnalystAgent : IExclusionAnalystAgent
{
    public ExclusionAnalysisResult Analyze(ClaimAdjudicationRequest request, CoverageMatchResult coverage)
    {
        if (!coverage.IsMatched)
            return new ExclusionAnalysisResult(false, [], "Skipped because no applicable coverage was matched.");

        var exclusions = coverage.PolicyVersion!.Exclusions.ToArray();
        if (exclusions.Length == 0)
            return new ExclusionAnalysisResult(false, [], "No structured exclusions are listed for the applicable policy version.");

        // Document retrieval is a later slice. Until claim evidence can be cited against policy text,
        // possible exclusion matches must be reviewed by a human rather than guessed from the description.
        return new ExclusionAnalysisResult(
            true,
            exclusions,
            $"{exclusions.Length} policy exclusion(s) require evidence review; no document evidence was available to confirm or rule them out.");
    }
}

