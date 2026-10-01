using DomainCopilot.Application.Contracts;
using DomainCopilot.Domain.Entities;

namespace DomainCopilot.Application.Claims;

public sealed class ClaimIntakeService(IClaimRepository claims)
{
    public async Task<Claim> ReceiveAsync(ClaimIntakeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var claim = new Claim(request.ClaimNumber, request.PolicyNumber, request.CoverageCode, request.LossDate, request.ClaimedAmount, request.Description);
        await claims.AddAsync(claim, cancellationToken);
        return claim;
    }
}
