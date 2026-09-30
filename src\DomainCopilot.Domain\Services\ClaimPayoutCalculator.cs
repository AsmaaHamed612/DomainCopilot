using DomainCopilot.Domain.Entities;
using DomainCopilot.Domain.ValueObjects;

namespace DomainCopilot.Domain.Services;

public sealed class ClaimPayoutCalculator
{
    public Money Calculate(Coverage coverage, Money claimedAmount)
    {
        if (coverage.Limit.Currency != claimedAmount.Currency)
            throw new InvalidOperationException("Coverage and claim currencies must match.");

        var coveredAmount = Math.Min(claimedAmount.Amount, coverage.Limit.Amount);
        var payout = Math.Max(0m, coveredAmount - coverage.Deductible.Amount);
        return new Money(payout, claimedAmount.Currency);
    }
}
