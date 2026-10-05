namespace DomainCopilot.Domain.Entities;

public sealed class AgentRun
{
    private readonly List<AgentRunStep> _steps = [];

    private AgentRun() { }

    public AgentRun(Guid claimId, Guid correlationId, DateTimeOffset startedAt)
    {
        if (claimId == Guid.Empty) throw new ArgumentException("Claim ID is required.", nameof(claimId));
        if (correlationId == Guid.Empty) throw new ArgumentException("Correlation ID is required.", nameof(correlationId));
        if (startedAt == default) throw new ArgumentException("Start time is required.", nameof(startedAt));

        Id = Guid.NewGuid();
        ClaimId = claimId;
        CorrelationId = correlationId;
        StartedAt = startedAt;
        Status = "Running";
    }

    public Guid Id { get; private set; }
    public Guid ClaimId { get; private set; }
    public Guid CorrelationId { get; private set; }
    public string Status { get; private set; } = "Running";
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public long? DurationMs { get; private set; }
    public IReadOnlyCollection<AgentRunStep> Steps => _steps;

    public void RecordStep(
        string agent,
        string eventType,
        string status,
        string summary,
        DateTimeOffset occurredAt,
        long? durationMs = null,
        string? tool = null,
        int? chunksRetrieved = null,
        int? tokensUsed = null,
        decimal? cost = null)
    {
        if (Status != "Running") throw new InvalidOperationException("A finished run cannot record more steps.");
        if (string.IsNullOrWhiteSpace(agent)) throw new ArgumentException("Agent name is required.", nameof(agent));
        if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("Event type is required.", nameof(eventType));
        if (string.IsNullOrWhiteSpace(status)) throw new ArgumentException("Step status is required.", nameof(status));
        if (summary is null) throw new ArgumentNullException(nameof(summary));
        if (durationMs < 0) throw new ArgumentOutOfRangeException(nameof(durationMs));
        if (chunksRetrieved < 0) throw new ArgumentOutOfRangeException(nameof(chunksRetrieved));
        if (tokensUsed < 0) throw new ArgumentOutOfRangeException(nameof(tokensUsed));
        if (cost < 0) throw new ArgumentOutOfRangeException(nameof(cost));

        _steps.Add(new AgentRunStep(Id, agent.Trim(), eventType.Trim(), status.Trim(), summary, occurredAt,
            durationMs, tool, chunksRetrieved, tokensUsed, cost));
    }

    public void Complete(DateTimeOffset completedAt) => Finish("Succeeded", completedAt);
    public void Cancel(DateTimeOffset completedAt) => Finish("Cancelled", completedAt);
    public void Fail(DateTimeOffset completedAt) => Finish("Failed", completedAt);

    private void Finish(string status, DateTimeOffset completedAt)
    {
        if (Status != "Running") throw new InvalidOperationException("The run has already finished.");
        if (completedAt < StartedAt) throw new ArgumentOutOfRangeException(nameof(completedAt));
        CompletedAt = completedAt;
        DurationMs = (long)(completedAt - StartedAt).TotalMilliseconds;
        Status = status;
    }
}

public sealed class AgentRunStep
{
    private AgentRunStep() { }

    internal AgentRunStep(
        Guid agentRunId,
        string agent,
        string eventType,
        string status,
        string summary,
        DateTimeOffset occurredAt,
        long? durationMs,
        string? tool,
        int? chunksRetrieved,
        int? tokensUsed,
        decimal? cost)
    {
        Id = Guid.NewGuid();
        AgentRunId = agentRunId;
        Agent = agent;
        EventType = eventType;
        Status = status;
        Summary = summary;
        OccurredAt = occurredAt;
        DurationMs = durationMs;
        Tool = tool;
        ChunksRetrieved = chunksRetrieved;
        TokensUsed = tokensUsed;
        Cost = cost;
    }

    public Guid Id { get; private set; }
    public Guid AgentRunId { get; private set; }
    public string Agent { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public long? DurationMs { get; private set; }
    public string? Tool { get; private set; }
    public int? ChunksRetrieved { get; private set; }
    public int? TokensUsed { get; private set; }
    public decimal? Cost { get; private set; }
}
