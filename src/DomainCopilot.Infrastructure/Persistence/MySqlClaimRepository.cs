using DomainCopilot.Application.Contracts;
using DomainCopilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Infrastructure.Persistence;

public sealed class MySqlClaimRepository(DomainCopilotDbContext dbContext) : IClaimRepository
{
    public async Task AddAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        await dbContext.Claims.AddAsync(claim, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Claims.SingleOrDefaultAsync(claim => claim.Id == id, cancellationToken);

    public async Task SaveAsync(Claim claim, CancellationToken cancellationToken = default)
    {
        dbContext.Claims.Update(claim);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
