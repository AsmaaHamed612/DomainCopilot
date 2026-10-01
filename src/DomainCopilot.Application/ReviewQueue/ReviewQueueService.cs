using DomainCopilot.Application.Contracts;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Domain.Enums;

namespace DomainCopilot.Application.ReviewQueue;

public sealed class ReviewQueueService
{
    private static readonly IReadOnlyDictionary<ReviewPriority, TimeSpan> ServiceLevels = new Dictionary<ReviewPriority, TimeSpan>
    {
        [ReviewPriority.Critical] = TimeSpan.FromHours(4),
        [ReviewPriority.High] = TimeSpan.FromHours(24),
        [ReviewPriority.Normal] = TimeSpan.FromHours(72),
        [ReviewPriority.Low] = TimeSpan.FromHours(120)
    };

    private readonly IReviewQueueRepository _repository;
    private readonly IClaimRepository _claims;
    private readonly TimeProvider _timeProvider;

    public ReviewQueueService(IReviewQueueRepository repository, IClaimRepository claims, TimeProvider timeProvider)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _claims = claims ?? throw new ArgumentNullException(nameof(claims));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<ReviewQueueItem> CreateAsync(Guid claimId, ReviewPriority priority, CancellationToken cancellationToken = default)
    {
        if (!ServiceLevels.TryGetValue(priority, out var sla))
            throw new ArgumentOutOfRangeException(nameof(priority));

        var now = _timeProvider.GetUtcNow();
        var item = new ReviewQueueItem(claimId, priority, now, sla);
        await _repository.AddAsync(item, cancellationToken);
        return item;
    }

    public Task<IReadOnlyList<ReviewQueueItem>> ListAsync(CancellationToken cancellationToken = default) =>
        _repository.ListAsync(cancellationToken);

    public async Task<ReviewQueueItem?> AssignAsync(Guid id, string reviewerId, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item is null) return null;

        item.Assign(reviewerId);
        await _repository.SaveAsync(item, cancellationToken);
        return item;
    }

    public async Task<ReviewQueueItem?> StartAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item is null) return null;

        item.StartReview();
        await _repository.SaveAsync(item, cancellationToken);
        return item;
    }

    public async Task<ReviewQueueItem?> EscalateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item is null) return null;

        item.Escalate(_timeProvider.GetUtcNow());
        await _repository.SaveAsync(item, cancellationToken);
        return item;
    }

    public async Task<ReviewQueueItem?> DecideAsync(
        Guid id,
        string reviewerId,
        ReviewDecisionAction action,
        string? editedDecision,
        string comment,
        CancellationToken cancellationToken = default)
    {
        var item = await _repository.GetByIdAsync(id, cancellationToken);
        if (item is null) return null;
        if (string.IsNullOrWhiteSpace(reviewerId)) throw new ArgumentException("Reviewer ID is required.", nameof(reviewerId));
        var normalizedReviewerId = reviewerId.Trim();
        if (!string.Equals(item.AssignedReviewerId, normalizedReviewerId, StringComparison.Ordinal))
            throw new InvalidOperationException("Only the assigned reviewer can decide this item.");

        var decision = action switch
        {
            ReviewDecisionAction.Approve => "Approved",
            ReviewDecisionAction.Reject => "Rejected",
            ReviewDecisionAction.EditAndApprove when !string.IsNullOrWhiteSpace(editedDecision) => editedDecision.Trim(),
            ReviewDecisionAction.EditAndApprove => throw new ArgumentException("Edited decision is required.", nameof(editedDecision)),
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };

        switch (action)
        {
            case ReviewDecisionAction.Approve:
                item.Approve(comment);
                break;
            case ReviewDecisionAction.Reject:
                item.Reject(comment);
                break;
            case ReviewDecisionAction.EditAndApprove:
                item.EditAndApprove(decision, comment);
                break;
        }

        var createdAt = _timeProvider.GetUtcNow();
        await _repository.SaveAsync(item, cancellationToken);
        var claim = await _claims.GetByIdAsync(item.ClaimId, cancellationToken);
        if (claim is null)
            throw new InvalidOperationException("The claim associated with this review item no longer exists.");
        if (action == ReviewDecisionAction.Reject) claim.Reject(); else claim.Approve();
        await _claims.SaveAsync(claim, cancellationToken);
        await _repository.AddDecisionAuditAsync(
            new ReviewDecisionAudit(item.Id, normalizedReviewerId, action.ToString(), decision, comment, createdAt),
            cancellationToken);
        return item;
    }

    public Task<IReadOnlyList<ReviewDecisionAudit>> ListDecisionAuditAsync(Guid id, CancellationToken cancellationToken = default) =>
        _repository.ListDecisionAuditAsync(id, cancellationToken);
}
