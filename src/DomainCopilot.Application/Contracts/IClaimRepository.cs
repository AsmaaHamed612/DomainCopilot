using DomainCopilot.Domain.Entities;

namespace DomainCopilot.Application.Contracts;

public interface IClaimRepository
{
    Task AddAsync(Claim claim, CancellationToken cancellationToken = default);
    Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveAsync(Claim claim, CancellationToken cancellationToken = default);
}
