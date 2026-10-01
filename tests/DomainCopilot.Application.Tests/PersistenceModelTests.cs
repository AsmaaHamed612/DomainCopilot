using DomainCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Application.Tests;

public sealed class PersistenceModelTests
{
    [Fact]
    public void SqlServerModel_CreatesClaimsPolicyReviewAndAuditTables()
    {
        var options = new DbContextOptionsBuilder<DomainCopilotDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ModelOnly;Trusted_Connection=True")
            .Options;

        using var dbContext = new DomainCopilotDbContext(options);
        var createScript = dbContext.Database.GenerateCreateScript();

        Assert.Contains("CREATE TABLE [Claims]", createScript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE [PolicyVersions]", createScript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE [Coverages]", createScript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE [ReviewQueueItems]", createScript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE [ReviewDecisionAudits]", createScript, StringComparison.OrdinalIgnoreCase);
    }
}
