using DomainCopilot.Domain.ValueObjects;

namespace DomainCopilot.Domain.Entities;

public sealed class PolicyVersion
{
    private readonly List<Coverage> _coverages = [];
    private readonly List<Exclusion> _exclusions = [];

    private PolicyVersion() { }

    public PolicyVersion(string policyNumber, int version, PolicyPeriod period)
    {
        ArgumentNullException.ThrowIfNull(period);
        if (string.IsNullOrWhiteSpace(policyNumber)) throw new ArgumentException("Policy number is required.", nameof(policyNumber));
        if (version <= 0) throw new ArgumentOutOfRangeException(nameof(version), "Version must be positive.");

        PolicyNumber = policyNumber.Trim();
        Version = version;
        Period = period;
    }

    public string PolicyNumber { get; private set; } = string.Empty;
    public int Version { get; private set; }
    public PolicyPeriod Period { get; private set; } = null!;
    public IReadOnlyCollection<Coverage> Coverages => _coverages.AsReadOnly();
    public IReadOnlyCollection<Exclusion> Exclusions => _exclusions.AsReadOnly();

    public void AddCoverage(Coverage coverage) => _coverages.Add(coverage ?? throw new ArgumentNullException(nameof(coverage)));
    public void AddExclusion(Exclusion exclusion) => _exclusions.Add(exclusion ?? throw new ArgumentNullException(nameof(exclusion)));
}
