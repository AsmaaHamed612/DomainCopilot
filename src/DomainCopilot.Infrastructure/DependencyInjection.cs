using DomainCopilot.Application.Claims;
using DomainCopilot.Application.Contracts;
using DomainCopilot.Application.ReviewQueue;
using DomainCopilot.Domain.Services;
using DomainCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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

        services.AddDbContext<DomainCopilotDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IPolicyRepository, SqlServerPolicyRepository>();
        services.AddScoped<IClaimRepository, SqlServerClaimRepository>();
        services.AddScoped<IReviewQueueRepository, SqlServerReviewQueueRepository>();
        services.AddScoped<IClaimAdjudicationService, ClaimAdjudicationService>();
        services.AddScoped<ClaimIntakeService>();
        services.AddScoped<ReviewQueueService>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ClaimPayoutCalculator>();
        return services;
    }
}
