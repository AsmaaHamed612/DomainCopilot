using DomainCopilot.Domain.ValueObjects;

namespace DomainCopilot.Domain.Entities;

public sealed class Coverage
{
    private Coverage() { }

    public Coverage(string code, string name, Money limit, Money deductible)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Coverage code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Coverage name is required.", nameof(name));
        if (deductible.Amount > limit.Amount) throw new ArgumentException("Deductible cannot exceed the coverage limit.", nameof(deductible));

        Code = code.Trim();
        Name = name.Trim();
        Limit = limit;
        Deductible = deductible;
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Money Limit { get; private set; }
    public Money Deductible { get; private set; }
}
