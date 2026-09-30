namespace DomainCopilot.Domain.ValueObjects;

public readonly struct PolicyPeriod : IEquatable<PolicyPeriod>
{
    public PolicyPeriod(DateOnly effectiveFrom, DateOnly effectiveTo)
    {
        if (effectiveTo < effectiveFrom)
            throw new ArgumentException("Policy end date cannot be before its start date.");

        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }

    public DateOnly EffectiveFrom { get; }
    public DateOnly EffectiveTo { get; }

    public bool Contains(DateOnly date) => date >= EffectiveFrom && date <= EffectiveTo;
    public bool Equals(PolicyPeriod other) => EffectiveFrom == other.EffectiveFrom && EffectiveTo == other.EffectiveTo;
    public override bool Equals(object? obj) => obj is PolicyPeriod other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(EffectiveFrom, EffectiveTo);
    public static bool operator ==(PolicyPeriod left, PolicyPeriod right) => left.Equals(right);
    public static bool operator !=(PolicyPeriod left, PolicyPeriod right) => !left.Equals(right);
}
