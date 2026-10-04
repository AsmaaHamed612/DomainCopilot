using DomainCopilot.Application.Claims;
using DomainCopilot.Application.Contracts;
using DomainCopilot.Application.ReviewQueue;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Domain.Enums;
using DomainCopilot.Domain.ValueObjects;
using DomainCopilot.Infrastructure;
using DomainCopilot.Infrastructure.Persistence;
using DomainCopilot.Api.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClaimTypes = System.Security.Claims.ClaimTypes;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddDomainCopilot(builder.Configuration);
builder.Services.AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.SchemeName, _ => { });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Adjuster", policy => policy.RequireRole("Adjuster"))
    .AddPolicy("Reviewer", policy => policy.RequireRole("Reviewer"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    var (statusCode, title) = error switch
    {
        ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
        InvalidOperationException => (StatusCodes.Status409Conflict, "Invalid workflow transition"),
        _ => (StatusCodes.Status500InternalServerError, "Unexpected server error")
    };

    context.Response.StatusCode = statusCode;
    await Results.Problem(title: title, statusCode: statusCode).ExecuteAsync(context);
}));

app.Use(async (context, next) =>
{
    var supplied = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
    var correlationId = Guid.TryParse(supplied, out var parsed) ? parsed : Guid.NewGuid();
    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers["X-Correlation-ID"] = correlationId.ToString();
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).WithName("HealthCheck");

app.MapPost("/claims", async (ClaimIntakeDto request, ClaimIntakeService intake, CancellationToken cancellationToken) =>
{
    var claim = await intake.ReceiveAsync(
        new ClaimIntakeRequest(
            request.ClaimNumber,
            request.PolicyNumber,
            request.CoverageCode,
            request.LossDate,
            new Money(request.ClaimedAmount, request.Currency),
            request.Description),
        cancellationToken);

    return Results.Created($"/claims/{claim.Id}", ToResponse(claim));
}).WithName("ReceiveClaim").RequireAuthorization("Adjuster");

app.MapPost("/claims/{id:guid}/adjudicate", async (
    Guid id, HttpContext context, IClaimRepository claims, IClaimAdjudicationService adjudication,
    ReviewQueueService reviewQueue, IAgentRunRepository runs, TimeProvider timeProvider,
    CancellationToken cancellationToken) => await RunClaimAdjudicationAsync(
        id, context, claims, adjudication, reviewQueue, runs, timeProvider, stream: false, cancellationToken))
    .WithName("AdjudicateClaim").RequireAuthorization("Adjuster");

app.MapPost("/claims/{id:guid}/adjudicate/stream", async (
    Guid id, HttpContext context, IClaimRepository claims, IClaimAdjudicationService adjudication,
    ReviewQueueService reviewQueue, IAgentRunRepository runs, TimeProvider timeProvider,
    CancellationToken cancellationToken) => await RunClaimAdjudicationAsync(
        id, context, claims, adjudication, reviewQueue, runs, timeProvider, stream: true, cancellationToken))
    .WithName("StreamClaimAdjudication").RequireAuthorization("Adjuster");

app.MapGet("/review-queue", async (ReviewQueueService service, CancellationToken cancellationToken) =>
{
    var items = await service.ListAsync(cancellationToken);
    return Results.Ok(items.Select(ToReviewResponse));
}).WithName("ListReviewQueue").RequireAuthorization("Reviewer");

app.MapPut("/review-queue/{id:guid}/assignment", async (
    Guid id,
    AssignReviewDto request,
    ReviewQueueService service,
    CancellationToken cancellationToken) =>
{
    var item = await service.AssignAsync(id, request.ReviewerId, cancellationToken);
    return item is null ? Results.NotFound() : Results.Ok(ToReviewResponse(item));
}).WithName("AssignReview").RequireAuthorization("Adjuster");

app.MapPost("/review-queue/{id:guid}/start", async (Guid id, HttpContext context, ReviewQueueService service, CancellationToken cancellationToken) =>
{
    var reviewerId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrWhiteSpace(reviewerId)) return Results.Forbid();
    var item = await service.StartAsync(id, reviewerId, cancellationToken);
    return item is null ? Results.NotFound() : Results.Ok(ToReviewResponse(item));
}).WithName("StartReview").RequireAuthorization("Reviewer");

app.MapPost("/review-queue/{id:guid}/escalate", async (Guid id, ReviewQueueService service, CancellationToken cancellationToken) =>
{
    var item = await service.EscalateAsync(id, cancellationToken);
    return item is null ? Results.NotFound() : Results.Ok(ToReviewResponse(item));
}).WithName("EscalateReview").RequireAuthorization("Reviewer");

app.MapPost("/review-queue/{id:guid}/decision", async (
    Guid id,
    HttpContext context,
    ReviewDecisionDto request,
    ReviewQueueService service,
    CancellationToken cancellationToken) =>
{
    var reviewerId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrWhiteSpace(reviewerId)) return Results.Forbid();
    var item = await service.DecideAsync(id, reviewerId, request.Action, request.EditedDecision, request.Comment, cancellationToken);
    return item is null ? Results.NotFound() : Results.Ok(ToReviewResponse(item));
}).WithName("DecideReview").RequireAuthorization("Reviewer");

app.MapGet("/review-queue/{id:guid}/audit", async (
    Guid id,
    ReviewQueueService service,
    CancellationToken cancellationToken) =>
{
    var audit = await service.ListDecisionAuditAsync(id, cancellationToken);
    return Results.Ok(audit);
}).WithName("ReviewDecisionAudit").RequireAuthorization("Reviewer");

app.MapGet("/agent-runs/{id:guid}", async (
    Guid id, IAgentRunRepository runs, CancellationToken cancellationToken) =>
{
    var run = await runs.GetByIdAsync(id, cancellationToken);
    return run is null ? Results.NotFound() : Results.Ok(ToAgentRunResponse(run));
}).WithName("GetAgentRun").RequireAuthorization("Reviewer");

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DomainCopilotDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();

static async Task<IResult> RunClaimAdjudicationAsync(
    Guid id,
    HttpContext context,
    IClaimRepository claims,
    IClaimAdjudicationService adjudication,
    ReviewQueueService reviewQueue,
    IAgentRunRepository runs,
    TimeProvider timeProvider,
    bool stream,
    CancellationToken cancellationToken)
{
    var claim = await claims.GetByIdAsync(id, cancellationToken);
    if (claim is null) return Results.NotFound();
    if (claim.Status != ClaimStatus.Received)
        return Results.Conflict(new { error = "Claim has already entered adjudication." });

    claim.MarkUnderReview();
    await claims.SaveAsync(claim, cancellationToken);

    var correlationId = context.Items["CorrelationId"] is Guid value ? value : Guid.NewGuid();
    var run = new AgentRun(claim.Id, correlationId, timeProvider.GetUtcNow());
    await runs.AddAsync(run, CancellationToken.None);
    ReviewQueueItem? queueItem = null;

    async Task EnsureHumanReviewAsync()
    {
        if (claim.Status == ClaimStatus.UnderReview)
        {
            claim.MarkPendingHumanApproval();
            await claims.SaveAsync(claim, CancellationToken.None);
        }

        queueItem ??= await reviewQueue.CreateAsync(claim.Id, ReviewPriority.Normal, CancellationToken.None);
    }

    try
    {
        if (stream)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "text/event-stream; charset=utf-8";
            context.Response.Headers.CacheControl = "no-cache";
            await context.Response.StartAsync(cancellationToken);
        }

        async ValueTask PublishProgressAsync(AgentProgressEvent progress, CancellationToken token)
        {
            run.RecordStep(progress.Agent, progress.EventType, progress.Status, progress.Summary,
                timeProvider.GetUtcNow(), progress.DurationMs);
            await runs.SaveAsync(run, CancellationToken.None);

            if (!stream) return;
            var payload = JsonSerializer.Serialize(new
            {
                runId = run.Id,
                correlationId,
                agent = progress.Agent,
                eventType = progress.EventType,
                status = progress.Status,
                summary = progress.Summary,
                progress.DurationMs
            });
            await context.Response.WriteAsync($"event: progress\ndata: {payload}\n\n", token);
            await context.Response.Body.FlushAsync(token);
        }

        var result = await adjudication.AdjudicateAsync(
            new ClaimAdjudicationRequest(claim.PolicyNumber, claim.LossDate, claim.CoverageCode, claim.ClaimedAmount),
            PublishProgressAsync,
            cancellationToken);

        // Every recommendation, including an abstention, is held for an adjuster decision.
        claim.MarkPendingHumanApproval();
        await claims.SaveAsync(claim, CancellationToken.None);
        queueItem = await reviewQueue.CreateAsync(claim.Id, ReviewPriority.Normal, CancellationToken.None);
        run.Complete(timeProvider.GetUtcNow());
        await runs.SaveAsync(run, CancellationToken.None);

        var response = new
        {
            claimId = claim.Id,
            runId = run.Id,
            correlationId,
            recommendation = result.Recommendation.ToString(),
            calculatedPayout = result.CalculatedPayout.Amount,
            currency = result.CalculatedPayout.Currency,
            result.Reason,
            result.PolicyVersion,
            agentSteps = result.AgentSteps,
            reviewQueueItemId = queueItem.Id,
            reviewStatus = queueItem.Status.ToString(),
            reviewDueAt = queueItem.DueAt
        };

        if (!stream) return Results.Ok(response);
        var finalPayload = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync($"event: result\ndata: {finalPayload}\n\n", cancellationToken);
        await context.Response.Body.FlushAsync(cancellationToken);
        return Results.Empty;
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        await EnsureHumanReviewAsync();
        if (run.Status == "Running")
        {
            run.Cancel(timeProvider.GetUtcNow());
            await runs.SaveAsync(run, CancellationToken.None);
        }
        throw;
    }
    catch (Exception)
    {
        await EnsureHumanReviewAsync();
        if (run.Status == "Running")
        {
            run.Fail(timeProvider.GetUtcNow());
            await runs.SaveAsync(run, CancellationToken.None);
        }
        if (stream && context.Response.HasStarted && !cancellationToken.IsCancellationRequested)
        {
            var errorPayload = JsonSerializer.Serialize(new { runId = run.Id, correlationId, status = "Failed", error = "adjudication_failed" });
            await context.Response.WriteAsync($"event: error\ndata: {errorPayload}\n\n", CancellationToken.None);
            await context.Response.Body.FlushAsync(CancellationToken.None);
            return Results.Empty;
        }
        throw;
    }
}

static ClaimResponse ToResponse(Claim claim) => new(
    claim.Id,
    claim.ClaimNumber,
    claim.PolicyNumber,
    claim.CoverageCode,
    claim.LossDate,
    claim.ClaimedAmount.Amount,
    claim.ClaimedAmount.Currency,
    claim.Description,
    claim.Status.ToString());

static ReviewQueueResponse ToReviewResponse(ReviewQueueItem item) => new(
    item.Id,
    item.ClaimId,
    item.AssignedReviewerId,
    item.Priority.ToString(),
    item.Status.ToString(),
    item.CreatedAt,
    item.DueAt,
    item.EscalatedAt,
    item.Decision,
    item.ReviewerComment);

static AgentRunResponse ToAgentRunResponse(AgentRun run) => new(
    run.Id,
    run.ClaimId,
    run.CorrelationId,
    run.Status,
    run.StartedAt,
    run.CompletedAt,
    run.DurationMs,
    run.Steps.OrderBy(step => step.OccurredAt).Select(step => new AgentRunStepResponse(
        step.Agent, step.EventType, step.Status, step.Summary, step.OccurredAt, step.DurationMs,
        step.Tool, step.ChunksRetrieved, step.TokensUsed, step.Cost)).ToArray());

public sealed record ClaimIntakeDto(string ClaimNumber, string PolicyNumber, string CoverageCode, DateOnly LossDate, decimal ClaimedAmount, string Currency, string Description);
public sealed record ClaimResponse(Guid Id, string ClaimNumber, string PolicyNumber, string CoverageCode, DateOnly LossDate, decimal ClaimedAmount, string Currency, string Description, string Status);
public sealed record AssignReviewDto(string ReviewerId);
public sealed record ReviewDecisionDto(ReviewDecisionAction Action, string? EditedDecision, string Comment);
public sealed record ReviewQueueResponse(Guid Id, Guid ClaimId, string? AssignedReviewerId, string Priority, string Status, DateTimeOffset CreatedAt, DateTimeOffset DueAt, DateTimeOffset? EscalatedAt, string? Decision, string? ReviewerComment);
public sealed record AgentRunStepResponse(string Agent, string EventType, string Status, string Summary, DateTimeOffset OccurredAt, long? DurationMs, string? Tool, int? ChunksRetrieved, int? TokensUsed, decimal? Cost);
public sealed record AgentRunResponse(Guid Id, Guid ClaimId, Guid CorrelationId, string Status, DateTimeOffset StartedAt, DateTimeOffset? CompletedAt, long? DurationMs, IReadOnlyList<AgentRunStepResponse> Steps);

public partial class Program;
