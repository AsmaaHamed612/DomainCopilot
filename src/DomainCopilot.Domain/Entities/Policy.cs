namespace DomainCopilot.Domain.Entities;

public sealed class Policy
{
    private readonly List<PolicyVersion> _versions = [];

    private Policy() { }

    public Policy(string policyNumber)
    {
        if (string.IsNullOrWhiteSpace(policyNumber)) throw new ArgumentException("Policy number is required.", nameof(policyNumber));
        PolicyNumber = policyNumber.Trim();
    }

    public string PolicyNumber { get; private set; } = string.Empty;
    public IReadOnlyCollection<PolicyVersion> Versions => _versions.AsReadOnly();

    public void AddVersion(PolicyVersion version) => _versions.Add(version ?? throw new ArgumentNullException(nameof(version)));

    public PolicyVersion? FindVersion(DateOnly lossDate) =>
        _versions
            .Where(v => v.Period.Contains(lossDate))
            .OrderByDescending(v => v.Version)
            .FirstOrDefault();
}
