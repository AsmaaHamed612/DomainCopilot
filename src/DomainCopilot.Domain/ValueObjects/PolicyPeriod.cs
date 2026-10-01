namespace DomainCopilot.Domain.ValueObjects;

public sealed class PolicyPeriod : IEquatable<PolicyPeriod>
{
    private PolicyPeriod() { }

    public PolicyPeriod(DateOnly effectiveFrom, DateOnly effectiveTo)
    {
        if (effectiveTo < effectiveFrom)
            throw new ArgumentException("Policy end date cannot be before its start date.");

        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }

    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly EffectiveTo { get; private set; }

    public bool Contains(DateOnly date) => date >= EffectiveFrom && date <= EffectiveTo;
    public bool Equals(PolicyPeriod? other) => other is not null && EffectiveFrom == other.EffectiveFrom && EffectiveTo == other.EffectiveTo;
    public override bool Equals(object? obj) => obj is PolicyPeriod other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(EffectiveFrom, EffectiveTo);
    public static bool operator ==(PolicyPeriod? left, PolicyPeriod? right) => Equals(left, right);
    public static bool operator !=(PolicyPeriod? left, PolicyPeriod? right) => !Equals(left, right);
}
