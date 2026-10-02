using DomainCopilot.Application.Claims;

namespace DomainCopilot.Application.Contracts;

public interface IAdjudicationDrafterAgent
{
    ClaimAdjudicationResult Draft(
        ClaimAdjudicationRequest request,
        CoverageMatchResult coverage,
        ExclusionAnalysisResult exclusions);
}

