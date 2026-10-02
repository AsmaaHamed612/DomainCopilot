using DomainCopilot.Application.Claims;

namespace DomainCopilot.Application.Contracts;

public interface IExclusionAnalystAgent
{
    ExclusionAnalysisResult Analyze(ClaimAdjudicationRequest request, CoverageMatchResult coverage);
}

