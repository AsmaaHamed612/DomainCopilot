using DomainCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DomainCopilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(DomainCopilotDbContext))]
[Migration("20261001090000_InitialClaimsReview")]
public sealed class InitialClaimsReview : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE `Policies` (
                `PolicyNumber` varchar(64) NOT NULL,
                CONSTRAINT `PK_Policies` PRIMARY KEY (`PolicyNumber`)
            ) ENGINE=InnoDB;

            CREATE TABLE `Claims` (
                `Id` char(36) NOT NULL,
                `ClaimNumber` varchar(64) NOT NULL,
                `PolicyNumber` varchar(64) NOT NULL,
                `CoverageCode` varchar(64) NOT NULL,
                `LossDate` date NOT NULL,
                `ClaimedAmount` decimal(18,2) NOT NULL,
                `Currency` varchar(3) NOT NULL,
                `Description` longtext NOT NULL,
                `Status` int NOT NULL,
                CONSTRAINT `PK_Claims` PRIMARY KEY (`Id`),
                UNIQUE INDEX `IX_Claims_ClaimNumber` (`ClaimNumber`)
            ) ENGINE=InnoDB;

            CREATE TABLE `ReviewQueueItems` (
                `Id` char(36) NOT NULL,
                `ClaimId` char(36) NOT NULL,
                `AssignedReviewerId` varchar(128) NULL,
                `Priority` int NOT NULL,
                `Status` int NOT NULL,
                `CreatedAt` bigint NOT NULL,
                `DueAt` bigint NOT NULL,
                `EscalatedAt` bigint NULL,
                `Decision` varchar(2048) NULL,
                `ReviewerComment` varchar(2048) NULL,
                CONSTRAINT `PK_ReviewQueueItems` PRIMARY KEY (`Id`),
                INDEX `IX_ReviewQueueItems_ClaimId` (`ClaimId`),
                CONSTRAINT `FK_ReviewQueueItems_Claims_ClaimId` FOREIGN KEY (`ClaimId`) REFERENCES `Claims` (`Id`) ON DELETE RESTRICT
            ) ENGINE=InnoDB;

            CREATE TABLE `PolicyVersions` (
                `PolicyNumber` varchar(64) NOT NULL,
                `Version` int NOT NULL,
                `EffectiveFrom` date NOT NULL,
                `EffectiveTo` date NOT NULL,
                CONSTRAINT `PK_PolicyVersions` PRIMARY KEY (`PolicyNumber`, `Version`),
                CONSTRAINT `FK_PolicyVersions_Policies_PolicyNumber` FOREIGN KEY (`PolicyNumber`) REFERENCES `Policies` (`PolicyNumber`) ON DELETE CASCADE
            ) ENGINE=InnoDB;

            CREATE TABLE `ReviewDecisionAudits` (
                `Id` char(36) NOT NULL,
                `ReviewQueueItemId` char(36) NOT NULL,
                `ReviewerId` varchar(128) NOT NULL,
                `Action` varchar(32) NOT NULL,
                `Decision` varchar(2048) NOT NULL,
                `Comment` varchar(2048) NOT NULL,
                `CreatedAt` bigint NOT NULL,
                CONSTRAINT `PK_ReviewDecisionAudits` PRIMARY KEY (`Id`),
                INDEX `IX_ReviewDecisionAudits_ReviewQueueItemId_CreatedAt` (`ReviewQueueItemId`, `CreatedAt`),
                CONSTRAINT `FK_ReviewDecisionAudits_ReviewQueueItems_ReviewQueueItemId` FOREIGN KEY (`ReviewQueueItemId`) REFERENCES `ReviewQueueItems` (`Id`) ON DELETE CASCADE
            ) ENGINE=InnoDB;

            CREATE TABLE `Coverages` (
                `PolicyNumber` varchar(64) NOT NULL,
                `PolicyVersion` int NOT NULL,
                `Code` varchar(64) NOT NULL,
                `Name` varchar(256) NOT NULL,
                `LimitAmount` decimal(18,2) NOT NULL,
                `LimitCurrency` varchar(3) NOT NULL,
                `DeductibleAmount` decimal(18,2) NOT NULL,
                `DeductibleCurrency` varchar(3) NOT NULL,
                CONSTRAINT `PK_Coverages` PRIMARY KEY (`PolicyNumber`, `PolicyVersion`, `Code`),
                CONSTRAINT `FK_Coverages_PolicyVersions_PolicyNumber_PolicyVersion` FOREIGN KEY (`PolicyNumber`, `PolicyVersion`) REFERENCES `PolicyVersions` (`PolicyNumber`, `Version`) ON DELETE CASCADE
            ) ENGINE=InnoDB;

            CREATE TABLE `Exclusions` (
                `PolicyNumber` varchar(64) NOT NULL,
                `PolicyVersion` int NOT NULL,
                `Code` varchar(64) NOT NULL,
                `Description` longtext NOT NULL,
                CONSTRAINT `PK_Exclusions` PRIMARY KEY (`PolicyNumber`, `PolicyVersion`, `Code`),
                CONSTRAINT `FK_Exclusions_PolicyVersions_PolicyNumber_PolicyVersion` FOREIGN KEY (`PolicyNumber`, `PolicyVersion`) REFERENCES `PolicyVersions` (`PolicyNumber`, `Version`) ON DELETE CASCADE
            ) ENGINE=InnoDB;

            INSERT INTO `Policies` (`PolicyNumber`) VALUES ('POL-DEMO-001');
            INSERT INTO `PolicyVersions` (`PolicyNumber`, `Version`, `EffectiveFrom`, `EffectiveTo`) VALUES
                ('POL-DEMO-001', 1, '2025-01-01', '2025-12-31'),
                ('POL-DEMO-001', 2, '2026-01-01', '2026-12-31');
            INSERT INTO `Coverages` (`PolicyNumber`, `PolicyVersion`, `Code`, `Name`, `LimitAmount`, `LimitCurrency`, `DeductibleAmount`, `DeductibleCurrency`) VALUES
                ('POL-DEMO-001', 1, 'WATER', 'Water damage', 100000.00, 'EGP', 5000.00, 'EGP'),
                ('POL-DEMO-001', 2, 'WATER', 'Water damage', 250000.00, 'EGP', 10000.00, 'EGP');
            INSERT INTO `Exclusions` (`PolicyNumber`, `PolicyVersion`, `Code`, `Description`) VALUES
                ('POL-DEMO-001', 2, 'FLOOD', 'Flood damage requires adjuster review in this synthetic sample policy.');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP TABLE IF EXISTS `ReviewDecisionAudits`;
            DROP TABLE IF EXISTS `Coverages`;
            DROP TABLE IF EXISTS `Exclusions`;
            DROP TABLE IF EXISTS `ReviewQueueItems`;
            DROP TABLE IF EXISTS `PolicyVersions`;
            DROP TABLE IF EXISTS `Claims`;
            DROP TABLE IF EXISTS `Policies`;
            """);
    }
}
