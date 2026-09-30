using DomainCopilot.Domain.Entities;
using DomainCopilot.Domain.Enums;

namespace DomainCopilot.Application.ReviewQueue;

public sealed class ReviewQueueService
{
    public ReviewQueueItem Create(Guid claimId, ReviewPriority priority, DateTimeOffset createdAt, TimeSpan sla) =>
        new(claimId, priority, createdAt, sla);
}
