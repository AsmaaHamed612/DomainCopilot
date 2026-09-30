using DomainCopilot.Application.Claims;

namespace DomainCopilot.Application.Contracts;

public interface IClaimAdjudicationService
{
    ClaimAdjudicationResult Adjudicate(ClaimAdjudicationRequest request);
}
