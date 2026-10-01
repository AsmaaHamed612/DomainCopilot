using DomainCopilot.Domain.Enums;
using DomainCopilot.Domain.ValueObjects;

namespace DomainCopilot.Domain.Entities;

public sealed class Claim
{
    private Claim() { }

    public Claim(string claimNumber, string policyNumber, string coverageCode, DateOnly lossDate, Money claimedAmount, string description)
    {
        if (string.IsNullOrWhiteSpace(claimNumber)) throw new ArgumentException("Claim number is required.", nameof(claimNumber));
        if (string.IsNullOrWhiteSpace(policyNumber)) throw new ArgumentException("Policy number is required.", nameof(policyNumber));
        if (string.IsNullOrWhiteSpace(coverageCode)) throw new ArgumentException("Coverage code is required.", nameof(coverageCode));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Claim description is required.", nameof(description));
        ArgumentNullException.ThrowIfNull(claimedAmount);

        Id = Guid.NewGuid();
        ClaimNumber = claimNumber.Trim();
        PolicyNumber = policyNumber.Trim();
        CoverageCode = coverageCode.Trim();
        LossDate = lossDate;
        ClaimedAmount = claimedAmount;
        Description = description.Trim();
        Status = ClaimStatus.Received;
    }

    public Guid Id { get; private set; }
    public string ClaimNumber { get; private set; } = string.Empty;
    public string PolicyNumber { get; private set; } = string.Empty;
    public string CoverageCode { get; private set; } = string.Empty;
    public DateOnly LossDate { get; private set; }
    public Money ClaimedAmount { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public ClaimStatus Status { get; private set; }

    public void MarkUnderReview() => Status = ClaimStatus.UnderReview;
    public void MarkPendingHumanApproval() => Status = ClaimStatus.PendingHumanApproval;
    public void Approve() => Status = ClaimStatus.Approved;
    public void Reject() => Status = ClaimStatus.Rejected;
}
