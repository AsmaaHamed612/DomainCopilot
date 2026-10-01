using DomainCopilot.Domain.Enums;

namespace DomainCopilot.Domain.Entities;

public sealed class ReviewQueueItem
{
    private ReviewQueueItem() { }

    public ReviewQueueItem(Guid claimId, ReviewPriority priority, DateTimeOffset createdAt, TimeSpan sla)
    {
        if (claimId == Guid.Empty) throw new ArgumentException("Claim ID is required.", nameof(claimId));
        if (sla <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(sla), "SLA must be positive.");

        Id = Guid.NewGuid();
        ClaimId = claimId;
        Priority = priority;
        Status = ReviewStatus.Pending;
        CreatedAt = createdAt;
        DueAt = createdAt.Add(sla);
    }

    public Guid Id { get; private set; }
    public Guid ClaimId { get; private set; }
    public string? AssignedReviewerId { get; private set; }
    public ReviewPriority Priority { get; private set; }
    public ReviewStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset DueAt { get; private set; }
    public DateTimeOffset? EscalatedAt { get; private set; }
    public string? Decision { get; private set; }
    public string? ReviewerComment { get; private set; }

    public bool IsOverdue(DateTimeOffset now) => now > DueAt && Status is not (ReviewStatus.Approved or ReviewStatus.Rejected);

    public void Assign(string reviewerId)
    {
        if (string.IsNullOrWhiteSpace(reviewerId)) throw new ArgumentException("Reviewer ID is required.", nameof(reviewerId));
        if (Status is ReviewStatus.Approved or ReviewStatus.Rejected)
            throw new InvalidOperationException("A completed review item cannot be reassigned.");
        AssignedReviewerId = reviewerId.Trim();
        Status = ReviewStatus.Assigned;
    }

    public void StartReview()
    {
        if (string.IsNullOrWhiteSpace(AssignedReviewerId)) throw new InvalidOperationException("A reviewer must be assigned first.");
        if (Status is not (ReviewStatus.Assigned or ReviewStatus.Escalated))
            throw new InvalidOperationException("The item must be assigned or escalated before review starts.");
        Status = ReviewStatus.InProgress;
    }

    public void Escalate(DateTimeOffset at)
    {
        if (!IsOverdue(at)) throw new InvalidOperationException("Only an overdue open item can be escalated.");
        EscalatedAt = at;
        Status = ReviewStatus.Escalated;
    }

    public void Approve(string comment)
    {
        ValidateComment(comment);
        EnsureReviewInProgress();
        Decision = "Approved";
        ReviewerComment = comment.Trim();
        Status = ReviewStatus.Approved;
    }

    public void Reject(string comment)
    {
        ValidateComment(comment);
        EnsureReviewInProgress();
        Decision = "Rejected";
        ReviewerComment = comment.Trim();
        Status = ReviewStatus.Rejected;
    }

    public void EditAndApprove(string decision, string comment)
    {
        if (string.IsNullOrWhiteSpace(decision)) throw new ArgumentException("Decision is required.", nameof(decision));
        ValidateComment(comment);
        EnsureReviewInProgress();
        Decision = decision.Trim();
        ReviewerComment = comment.Trim();
        Status = ReviewStatus.Approved;
    }

    private static void ValidateComment(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment)) throw new ArgumentException("Reviewer comment is required.", nameof(comment));
    }

    private void EnsureReviewInProgress()
    {
        if (Status != ReviewStatus.InProgress)
            throw new InvalidOperationException("A review must be in progress before a decision is recorded.");
    }
}
