using DomainCopilot.Domain.Entities;

namespace DomainCopilot.Application.Contracts;

public interface IAgentRunRepository
{
    Task AddAsync(AgentRun run, CancellationToken cancellationToken = default);
    Task SaveAsync(AgentRun run, CancellationToken cancellationToken = default);
    Task<AgentRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
