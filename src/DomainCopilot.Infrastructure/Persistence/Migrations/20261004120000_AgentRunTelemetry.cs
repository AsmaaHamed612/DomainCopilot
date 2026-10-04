using DomainCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DomainCopilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(DomainCopilotDbContext))]
[Migration("20261004120000_AgentRunTelemetry")]
public sealed class AgentRunTelemetry : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE `AgentRuns` (
                `Id` char(36) NOT NULL,
                `ClaimId` char(36) NOT NULL,
                `CorrelationId` char(36) NOT NULL,
                `Status` varchar(32) NOT NULL,
                `StartedAt` bigint NOT NULL,
                `CompletedAt` bigint NULL,
                `DurationMs` bigint NULL,
                CONSTRAINT `PK_AgentRuns` PRIMARY KEY (`Id`),
                INDEX `IX_AgentRuns_ClaimId_StartedAt` (`ClaimId`, `StartedAt`),
                INDEX `IX_AgentRuns_CorrelationId` (`CorrelationId`),
                CONSTRAINT `FK_AgentRuns_Claims_ClaimId` FOREIGN KEY (`ClaimId`) REFERENCES `Claims` (`Id`) ON DELETE RESTRICT
            ) ENGINE=InnoDB;

            CREATE TABLE `AgentRunSteps` (
                `Id` char(36) NOT NULL,
                `AgentRunId` char(36) NOT NULL,
                `Agent` varchar(128) NOT NULL,
                `EventType` varchar(64) NOT NULL,
                `Status` varchar(32) NOT NULL,
                `Summary` longtext NOT NULL,
                `OccurredAt` bigint NOT NULL,
                `DurationMs` bigint NULL,
                `Tool` varchar(128) NULL,
                `ChunksRetrieved` int NULL,
                `TokensUsed` int NULL,
                `Cost` decimal(18,6) NULL,
                CONSTRAINT `PK_AgentRunSteps` PRIMARY KEY (`Id`),
                INDEX `IX_AgentRunSteps_AgentRunId_OccurredAt` (`AgentRunId`, `OccurredAt`),
                CONSTRAINT `FK_AgentRunSteps_AgentRuns_AgentRunId` FOREIGN KEY (`AgentRunId`) REFERENCES `AgentRuns` (`Id`) ON DELETE CASCADE
            ) ENGINE=InnoDB;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE IF EXISTS `AgentRunSteps`;
            DROP TABLE IF EXISTS `AgentRuns`;
            """);
    }
}
