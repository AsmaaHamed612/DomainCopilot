using DomainCopilot.Domain.ValueObjects;

namespace DomainCopilot.Application.Claims;

public sealed record ClaimIntakeRequest(
    string ClaimNumber,
    string PolicyNumber,
    string CoverageCode,
    DateOnly LossDate,
    Money ClaimedAmount,
    string Description);
