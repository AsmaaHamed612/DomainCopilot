namespace DomainCopilot.Domain.Entities;

public sealed class Exclusion
{
    private Exclusion() { }

    public Exclusion(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Exclusion code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Exclusion description is required.", nameof(description));

        Code = code.Trim();
        Description = description.Trim();
    }

    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
}
