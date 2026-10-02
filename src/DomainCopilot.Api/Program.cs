using DomainCopilot.Application.Claims;
using DomainCopilot.Application.Contracts;
using DomainCopilot.Application.ReviewQueue;
using DomainCopilot.Domain.Entities;
using DomainCopilot.Domain.Enums;
using DomainCopilot.Domain.ValueObjects;
using DomainCopilot.Infrastructure;
using DomainCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddOpenApi();
builder.Services.AddDomainCopilot(builder.Configuration);

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
}).WithName("ReceiveClaim");

app.MapPost("/claims/{id:guid}/adjudicate", async (
    Guid id,
    IClaimRepository claims,
    DomainCopilot.Application.Contracts.IClaimAdjudicationService adjudication,
    ReviewQueueService reviewQueue,
    CancellationToken cancellationToken) =>
{
    var claim = await claims.GetByIdAsync(id, cancellationToken);
    if (claim is null) return Results.NotFound();
    if (claim.Status != ClaimStatus.Received) return Results.Conflict(new { error = "Claim has already entered adjudication." });

    claim.MarkUnderReview();
    await claims.SaveAsync(claim, cancellationToken);

    var result = await adjudication.AdjudicateAsync(
        new ClaimAdjudicationRequest(claim.PolicyNumber, claim.LossDate, claim.CoverageCode, claim.ClaimedAmount),
        cancellationToken);

    // Every recommendation, including an abstention, is held for an adjuster decision.
    claim.MarkPendingHumanApproval();
    await claims.SaveAsync(claim, cancellationToken);
    var item = await reviewQueue.CreateAsync(claim.Id, ReviewPriority.Normal, cancellationToken);

    return Results.Ok(new
    {
        claimId = claim.Id,
        recommendation = result.Recommendation.ToString(),
        calculatedPayout = result.CalculatedPayout.Amount,
        currency = result.CalculatedPayout.Currency,
        result.Reason,
        result.PolicyVersion,
        agentSteps = result.AgentSteps,
        reviewQueueItemId = item.Id,
        reviewStatus = item.Status.ToString(),
        reviewDueAt = item.DueAt
    });
}).WithName("AdjudicateClaim");

app.MapGet("/review-queue", async (ReviewQueueService service, CancellationToken cancellationToken) =>
{
    var items = await service.ListAsync(cancellationToken);
    return Results.Ok(items.Select(ToReviewResponse));
}).WithName("ListReviewQueue");

app.MapPut("/review-queue/{id:guid}/assignment", async (
    Guid id,
    AssignReviewDto request,
    ReviewQueueService service,
    CancellationToken cancellationToken) =>
{
    var item = await service.AssignAsync(id, request.ReviewerId, cancellationToken);
    return item is null ? Results.NotFound() : Results.Ok(ToReviewResponse(item));
}).WithName("AssignReview");

app.MapPost("/review-queue/{id:guid}/start", async (Guid id, ReviewQueueService service, CancellationToken cancellationToken) =>
{
    var item = await service.StartAsync(id, cancellationToken);
    return item is null ? Results.NotFound() : Results.Ok(ToReviewResponse(item));
}).WithName("StartReview");

app.MapPost("/review-queue/{id:guid}/escalate", async (Guid id, ReviewQueueService service, CancellationToken cancellationToken) =>
{
    var item = await service.EscalateAsync(id, cancellationToken);
    return item is null ? Results.NotFound() : Results.Ok(ToReviewResponse(item));
}).WithName("EscalateReview");

app.MapPost("/review-queue/{id:guid}/decision", async (
    Guid id,
    ReviewDecisionDto request,
    ReviewQueueService service,
    CancellationToken cancellationToken) =>
{
    var item = await service.DecideAsync(id, request.ReviewerId, request.Action, request.EditedDecision, request.Comment, cancellationToken);
    return item is null ? Results.NotFound() : Results.Ok(ToReviewResponse(item));
}).WithName("DecideReview");

app.MapGet("/review-queue/{id:guid}/audit", async (
    Guid id,
    ReviewQueueService service,
    CancellationToken cancellationToken) =>
{
    var audit = await service.ListDecisionAuditAsync(id, cancellationToken);
    return Results.Ok(audit);
}).WithName("ReviewDecisionAudit");

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DomainCopilotDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();

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

public sealed record ClaimIntakeDto(string ClaimNumber, string PolicyNumber, string CoverageCode, DateOnly LossDate, decimal ClaimedAmount, string Currency, string Description);
public sealed record ClaimResponse(Guid Id, string ClaimNumber, string PolicyNumber, string CoverageCode, DateOnly LossDate, decimal ClaimedAmount, string Currency, string Description, string Status);
public sealed record AssignReviewDto(string ReviewerId);
public sealed record ReviewDecisionDto(string ReviewerId, ReviewDecisionAction Action, string? EditedDecision, string Comment);
public sealed record ReviewQueueResponse(Guid Id, Guid ClaimId, string? AssignedReviewerId, string Priority, string Status, DateTimeOffset CreatedAt, DateTimeOffset DueAt, DateTimeOffset? EscalatedAt, string? Decision, string? ReviewerComment);

public partial class Program;

