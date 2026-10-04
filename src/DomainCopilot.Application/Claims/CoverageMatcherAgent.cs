using DomainCopilot.Application.Contracts;

namespace DomainCopilot.Application.Claims;

public sealed class CoverageMatcherAgent(IPolicyRepository policies) : ICoverageMatcherAgent
{
    public async Task<CoverageMatchResult> MatchAsync(
        ClaimAdjudicationRequest request,
        CancellationToken cancellationToken = default)
    {
        var policy = await policies.GetByPolicyNumberAsync(request.PolicyNumber.Trim(), cancellationToken);
        if (policy is null)
            return new CoverageMatchResult(null, null, "No matching policy was found.");

        var version = policy.FindVersion(request.LossDate);
        if (version is null)
            return new CoverageMatchResult(null, null, "No policy version covers the loss date.");

        var coverage = version.Coverages.FirstOrDefault(item =>
            string.Equals(item.Code, request.CoverageCode.Trim(), StringComparison.OrdinalIgnoreCase));
        if (coverage is null)
            return new CoverageMatchResult(version, null, "The applicable policy version does not list the requested coverage.");

        return new CoverageMatchResult(version, coverage,
            $"Matched policy {policy.PolicyNumber}, version {version.Version}, effective on the loss date.");
    }
}

