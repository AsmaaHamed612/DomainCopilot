using DomainCopilot.Application.Contracts;
using DomainCopilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Infrastructure.Persistence;

public sealed class SqlServerPolicyRepository(DomainCopilotDbContext dbContext) : IPolicyRepository
{
    public Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default) =>
        dbContext.Policies
            .Include(policy => policy.Versions)
            .ThenInclude(version => version.Coverages)
            .Include(policy => policy.Versions)
            .ThenInclude(version => version.Exclusions)
            .AsNoTracking()
            .SingleOrDefaultAsync(policy => policy.PolicyNumber == policyNumber, cancellationToken);
}
