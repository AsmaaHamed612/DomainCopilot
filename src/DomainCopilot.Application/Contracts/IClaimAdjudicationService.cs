using DomainCopilot.Application.Claims;

namespace DomainCopilot.Application.Contracts;

public interface IClaimAdjudicationService
{
    Task<ClaimAdjudicationResult> AdjudicateAsync(ClaimAdjudicationRequest request, CancellationToken cancellationToken = default);
}
