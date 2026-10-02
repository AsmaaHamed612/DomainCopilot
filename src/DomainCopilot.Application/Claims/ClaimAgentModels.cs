using DomainCopilot.Domain.Entities;
using DomainCopilot.Domain.Enums;
using DomainCopilot.Domain.ValueObjects;

namespace DomainCopilot.Application.Claims;

public sealed record CoverageMatchResult(
    PolicyVersion? PolicyVersion,
    Coverage? Coverage,
    string Summary)
{
    public bool IsMatched => PolicyVersion is not null && Coverage is not null;
}

public sealed record ExclusionAnalysisResult(
    bool RequiresHumanReview,
    IReadOnlyList<Exclusion> PotentialExclusions,
    string Summary);

public sealed record AgentStepResult(string Agent, string Status, string Summary);

