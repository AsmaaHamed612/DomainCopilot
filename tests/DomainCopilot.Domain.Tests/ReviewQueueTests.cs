using DomainCopilot.Domain.Entities;
using DomainCopilot.Domain.Enums;

namespace DomainCopilot.Domain.Tests;

public sealed class ReviewQueueTests
{
    [Fact]
    public void NewReviewItem_HasSlaDueDate()
    {
        var created = new DateTimeOffset(2026, 9, 30, 10, 0, 0, TimeSpan.Zero);
        var item = new ReviewQueueItem(Guid.NewGuid(), ReviewPriority.High, created, TimeSpan.FromHours(4));

        Assert.Equal(created.AddHours(4), item.DueAt);
        Assert.Equal(ReviewStatus.Pending, item.Status);
    }

    [Fact]
    public void ReviewItem_CanBeAssignedAndApproved()
    {
        var item = new ReviewQueueItem(Guid.NewGuid(), ReviewPriority.High, DateTimeOffset.UtcNow, TimeSpan.FromHours(4));

        item.Assign("adjuster-001");
        item.StartReview();
        item.Approve("Evidence supports the recommendation.");

        Assert.Equal(ReviewStatus.Approved, item.Status);
        Assert.Equal("adjuster-001", item.AssignedReviewerId);
    }

    [Fact]
    public void ApprovalRequiresReviewerComment()
    {
        var item = new ReviewQueueItem(Guid.NewGuid(), ReviewPriority.Normal, DateTimeOffset.UtcNow, TimeSpan.FromHours(4));

        Assert.Throws<ArgumentException>(() => item.Approve(""));
    }

    [Fact]
    public void OverduePendingItem_IsDetected()
    {
        var created = new DateTimeOffset(2026, 9, 30, 10, 0, 0, TimeSpan.Zero);
        var item = new ReviewQueueItem(Guid.NewGuid(), ReviewPriority.Critical, created, TimeSpan.FromHours(4));

        Assert.True(item.IsOverdue(created.AddHours(5)));
    }
}
