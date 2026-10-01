using DomainCopilot.Domain.Entities;

namespace DomainCopilot.Application.Contracts;

public interface IReviewQueueRepository
{
    Task AddAsync(ReviewQueueItem item, CancellationToken cancellationToken = default);
    Task<ReviewQueueItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReviewQueueItem>> ListAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(ReviewQueueItem item, CancellationToken cancellationToken = default);
    Task AddDecisionAuditAsync(ReviewDecisionAudit decision, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReviewDecisionAudit>> ListDecisionAuditAsync(Guid reviewQueueItemId, CancellationToken cancellationToken = default);
}
