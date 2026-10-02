using DomainCopilot.Application.Claims;
using DomainCopilot.Application.Contracts;
using DomainCopilot.Application.ReviewQueue;
using DomainCopilot.Domain.Services;
using DomainCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DomainCopilot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDomainCopilot(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DomainCopilot");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:DomainCopilot is required.");

        var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
        // The initial schema migration is hand-authored SQL rather than an EF-generated snapshot.
        services.AddDbContext<DomainCopilotDbContext>(options => options
            .UseMySql(connectionString, serverVersion)
            .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));
        services.AddScoped<IPolicyRepository, MySqlPolicyRepository>();
        services.AddScoped<IClaimRepository, MySqlClaimRepository>();
        services.AddScoped<IReviewQueueRepository, MySqlReviewQueueRepository>();
        services.AddScoped<ICoverageMatcherAgent, CoverageMatcherAgent>();
        services.AddScoped<IExclusionAnalystAgent, ExclusionAnalystAgent>();
        services.AddScoped<IAdjudicationDrafterAgent, AdjudicationDrafterAgent>();
        services.AddScoped<IClaimAdjudicationService, ClaimAdjudicationOrchestrator>();
        services.AddScoped<ClaimIntakeService>();
        services.AddScoped<ReviewQueueService>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ClaimPayoutCalculator>();
        return services;
    }
}

