using DomainCopilot.Application.Contracts;
using DomainCopilot.Application.ReviewQueue;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Domain.Enums;
using DomainCopilot.Domain.ValueObjects;

namespace DomainCopilot.Application.Tests;

public sealed class ReviewQueueServiceTests
{
    [Fact]
    public async Task DecideAsync_RecordsAuditAndUpdatesClaimAfterAssignedReviewerActs()
    {
        var now = new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);
        var claim = new Claim("CLM-001", "POL-001", "WATER", new DateOnly(2026, 9, 1), new Money(1000m, "EGP"), "Synthetic water damage");
        var claims = new FakeClaimRepository(claim);
        var queue = new FakeReviewQueueRepository();
        var service = new ReviewQueueService(queue, claims, new FixedTimeProvider(now));
        var item = await service.CreateAsync(claim.Id, ReviewPriority.High);
        await service.AssignAsync(item.Id, "adjuster-001");
        await service.StartAsync(item.Id);

        await service.DecideAsync(item.Id, "adjuster-001", ReviewDecisionAction.Approve, null, "Evidence reviewed.");

        Assert.Equal(ClaimStatus.Approved, claim.Status);
        Assert.Single(queue.Decisions);
        Assert.Equal("adjuster-001", queue.Decisions[0].ReviewerId);
        Assert.Equal("Evidence reviewed.", queue.Decisions[0].Comment);
    }

    [Fact]
    public async Task DecideAsync_RejectsDecisionFromReviewerOtherThanAssignee()
    {
        var now = new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero);
        var claim = new Claim("CLM-002", "POL-001", "WATER", new DateOnly(2026, 9, 1), new Money(1000m, "EGP"), "Synthetic water damage");
        var queue = new FakeReviewQueueRepository();
        var service = new ReviewQueueService(queue, new FakeClaimRepository(claim), new FixedTimeProvider(now));
        var item = await service.CreateAsync(claim.Id, ReviewPriority.Normal);
        await service.AssignAsync(item.Id, "adjuster-001");
        await service.StartAsync(item.Id);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.DecideAsync(item.Id, "adjuster-002", ReviewDecisionAction.Approve, null, "Not assigned."));
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeClaimRepository(Claim claim) : IClaimRepository
    {
        public Task AddAsync(Claim newClaim, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Claim?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(id == claim.Id ? claim : null);
        public Task SaveAsync(Claim savedClaim, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeReviewQueueRepository : IReviewQueueRepository
    {
        private readonly Dictionary<Guid, ReviewQueueItem> _items = [];
        public List<ReviewDecisionAudit> Decisions { get; } = [];

        public Task AddAsync(ReviewQueueItem item, CancellationToken cancellationToken = default)
        {
            _items.Add(item.Id, item);
            return Task.CompletedTask;
        }

        public Task<ReviewQueueItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.GetValueOrDefault(id));

        public Task<IReadOnlyList<ReviewQueueItem>> ListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ReviewQueueItem>>(_items.Values.ToArray());

        public Task SaveAsync(ReviewQueueItem item, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task AddDecisionAuditAsync(ReviewDecisionAudit decision, CancellationToken cancellationToken = default)
        {
            Decisions.Add(decision);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ReviewDecisionAudit>> ListDecisionAuditAsync(Guid reviewQueueItemId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ReviewDecisionAudit>>(Decisions.Where(item => item.ReviewQueueItemId == reviewQueueItemId).ToArray());
    }
}
