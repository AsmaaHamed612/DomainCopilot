using DomainCopilot.Application.Claims;

namespace DomainCopilot.Application.Contracts;

public interface ICoverageMatcherAgent
{
    Task<CoverageMatchResult> MatchAsync(ClaimAdjudicationRequest request, CancellationToken cancellationToken = default);
}

