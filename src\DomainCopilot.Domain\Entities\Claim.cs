using DomainCopilot.Domain.Enums;
using DomainCopilot.Domain.ValueObjects;

namespace DomainCopilot.Domain.Entities;

public sealed class Claim
{
    private Claim() { }

    public Claim(string claimNumber, string policyNumber, DateOnly lossDate, Money claimedAmount, string description)
    {
        if (string.IsNullOrWhiteSpace(claimNumber)) throw new ArgumentException("Claim number is required.", nameof(claimNumber));
        if (string.IsNullOrWhiteSpace(policyNumber)) throw new ArgumentException("Policy number is required.", nameof(policyNumber));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Claim description is required.", nameof(description));

        ClaimNumber = claimNumber.Trim();
        PolicyNumber = policyNumber.Trim();
        LossDate = lossDate;
        ClaimedAmount = claimedAmount;
        Description = description.Trim();
        Status = ClaimStatus.Received;
    }

    public string ClaimNumber { get; private set; } = string.Empty;
    public string PolicyNumber { get; private set; } = string.Empty;
    public DateOnly LossDate { get; private set; }
    public Money ClaimedAmount { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public ClaimStatus Status { get; private set; }

    public void MarkUnderReview() => Status = ClaimStatus.UnderReview;
    public void MarkPendingHumanApproval() => Status = ClaimStatus.PendingHumanApproval;
    public void Approve() => Status = ClaimStatus.Approved;
    public void Reject() => Status = ClaimStatus.Rejected;
}
