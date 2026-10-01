using DomainCopilot.Domain.Entities;

namespace DomainCopilot.Application.Contracts;

public interface IPolicyRepository
{
    Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default);
}
