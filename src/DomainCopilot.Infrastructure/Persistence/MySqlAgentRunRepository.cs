using DomainCopilot.Application.Contracts;
using DomainCopilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Infrastructure.Persistence;

public sealed class MySqlAgentRunRepository(DomainCopilotDbContext dbContext) : IAgentRunRepository
{
    public async Task AddAsync(AgentRun run, CancellationToken cancellationToken = default)
    {
        await dbContext.AgentRuns.AddAsync(run, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(AgentRun run, CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<AgentRun?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.AgentRuns.Include(run => run.Steps)
            .SingleOrDefaultAsync(run => run.Id == id, cancellationToken);
}
