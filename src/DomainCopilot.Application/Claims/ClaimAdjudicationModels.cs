using DomainCopilot.Domain.Enums;
using DomainCopilot.Domain.ValueObjects;

namespace DomainCopilot.Application.Claims;

public sealed record ClaimAdjudicationRequest(
    string PolicyNumber,
    DateOnly LossDate,
    string CoverageCode,
    Money ClaimedAmount);

public sealed record ClaimAdjudicationResult(
    RecommendationType Recommendation,
    Money CalculatedPayout,
    string Reason,
    int? PolicyVersion);
