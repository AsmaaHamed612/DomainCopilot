using DomainCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Application.Tests;

public sealed class PersistenceModelTests
{
    [Fact]
    public void MySqlModel_CreatesClaimsPolicyReviewAndAuditTables()
    {
        var options = new DbContextOptionsBuilder<DomainCopilotDbContext>()
            .UseMySql("Server=localhost;Database=ModelOnly;User=root", new MySqlServerVersion(new Version(8, 0, 0)))
            .Options;

        using var dbContext = new DomainCopilotDbContext(options);
        var createScript = dbContext.Database.GenerateCreateScript();

        Assert.Contains("CREATE TABLE `Claims`", createScript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE `PolicyVersions`", createScript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE `Coverages`", createScript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE `ReviewQueueItems`", createScript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CREATE TABLE `ReviewDecisionAudits`", createScript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("`CreatedAt` bigint", createScript, StringComparison.OrdinalIgnoreCase);
    }
}
