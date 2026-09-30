using DomainCopilot.Application.Contracts;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Domain.Enums;
using DomainCopilot.Domain.Services;

namespace DomainCopilot.Application.Claims;

public sealed class ClaimAdjudicationService : IClaimAdjudicationService
{
    private readonly ClaimPayoutCalculator _calculator;

    public ClaimAdjudicationService(ClaimPayoutCalculator calculator)
    {
        _calculator = calculator;
    }

    public ClaimAdjudicationResult Adjudicate(ClaimAdjudicationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        throw new NotImplementedException("Policy retrieval will be implemented in the next Day 1/Day 2 slice.");
    }
}
