namespace DomainCopilot.Domain.Entities;

public sealed class ReviewDecisionAudit
{
    private ReviewDecisionAudit() { }

    public ReviewDecisionAudit(Guid reviewQueueItemId, string reviewerId, string action, string decision, string comment, DateTimeOffset createdAt)
    {
        if (reviewQueueItemId == Guid.Empty) throw new ArgumentException("Review item ID is required.", nameof(reviewQueueItemId));
        if (string.IsNullOrWhiteSpace(reviewerId)) throw new ArgumentException("Reviewer ID is required.", nameof(reviewerId));
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action is required.", nameof(action));
        if (string.IsNullOrWhiteSpace(decision)) throw new ArgumentException("Decision is required.", nameof(decision));
        if (string.IsNullOrWhiteSpace(comment)) throw new ArgumentException("Reviewer comment is required.", nameof(comment));

        Id = Guid.NewGuid();
        ReviewQueueItemId = reviewQueueItemId;
        ReviewerId = reviewerId.Trim();
        Action = action.Trim();
        Decision = decision.Trim();
        Comment = comment.Trim();
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid ReviewQueueItemId { get; private set; }
    public string ReviewerId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string Decision { get; private set; } = string.Empty;
    public string Comment { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
}
