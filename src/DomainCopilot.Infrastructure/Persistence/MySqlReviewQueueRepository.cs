using DomainCopilot.Application.Contracts;
using DomainCopilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Infrastructure.Persistence;

public sealed class MySqlReviewQueueRepository(DomainCopilotDbContext dbContext) : IReviewQueueRepository
{
    public async Task AddAsync(ReviewQueueItem item, CancellationToken cancellationToken = default)
    {
        await dbContext.ReviewQueueItems.AddAsync(item, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<ReviewQueueItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.ReviewQueueItems.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ReviewQueueItem>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.ReviewQueueItems
            .AsNoTracking()
            .OrderBy(item => item.Status)
            .ThenBy(item => item.DueAt)
            .ToListAsync(cancellationToken);

    public async Task SaveAsync(ReviewQueueItem item, CancellationToken cancellationToken = default)
    {
        dbContext.ReviewQueueItems.Update(item);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddDecisionAuditAsync(ReviewDecisionAudit decision, CancellationToken cancellationToken = default)
    {
        await dbContext.ReviewDecisionAudits.AddAsync(decision, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReviewDecisionAudit>> ListDecisionAuditAsync(Guid reviewQueueItemId, CancellationToken cancellationToken = default) =>
        await dbContext.ReviewDecisionAudits
            .AsNoTracking()
            .Where(decision => decision.ReviewQueueItemId == reviewQueueItemId)
            .OrderBy(decision => decision.CreatedAt)
            .ToListAsync(cancellationToken);
}
