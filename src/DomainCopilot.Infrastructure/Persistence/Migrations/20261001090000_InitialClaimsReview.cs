using DomainCopilot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DomainCopilot.Infrastructure.Persistence.Migrations;

[DbContext(typeof(DomainCopilotDbContext))]
[Migration("20261001090000_InitialClaimsReview")]
public sealed class InitialClaimsReview : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Claims",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClaimNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                PolicyNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                CoverageCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                LossDate = table.Column<DateOnly>(type: "date", nullable: false),
                ClaimedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Claims", row => row.Id));

        migrationBuilder.CreateTable(
            name: "Policies",
            columns: table => new { PolicyNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false) },
            constraints: table => table.PrimaryKey("PK_Policies", row => row.PolicyNumber));

        migrationBuilder.CreateTable(
            name: "ReviewQueueItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ClaimId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AssignedReviewerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                Priority = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                DueAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                EscalatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                Decision = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                ReviewerComment = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReviewQueueItems", row => row.Id);
                table.ForeignKey("FK_ReviewQueueItems_Claims_ClaimId", row => row.ClaimId, "Claims", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "PolicyVersions",
            columns: table => new
            {
                PolicyNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Version = table.Column<int>(type: "int", nullable: false),
                EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                EffectiveTo = table.Column<DateOnly>(type: "date", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PolicyVersions", row => new { row.PolicyNumber, row.Version });
                table.ForeignKey("FK_PolicyVersions_Policies_PolicyNumber", row => row.PolicyNumber, "Policies", "PolicyNumber", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ReviewDecisionAudits",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReviewQueueItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReviewerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Action = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                Decision = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                Comment = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReviewDecisionAudits", row => row.Id);
                table.ForeignKey("FK_ReviewDecisionAudits_ReviewQueueItems_ReviewQueueItemId", row => row.ReviewQueueItemId, "ReviewQueueItems", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Coverages",
            columns: table => new
            {
                PolicyNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                PolicyVersion = table.Column<int>(type: "int", nullable: false),
                Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                LimitAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                LimitCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                DeductibleAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                DeductibleCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Coverages", row => new { row.PolicyNumber, row.PolicyVersion, row.Code });
                table.ForeignKey("FK_Coverages_PolicyVersions_PolicyNumber_PolicyVersion", row => new { row.PolicyNumber, row.PolicyVersion }, "PolicyVersions", new[] { "PolicyNumber", "Version" }, onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Exclusions",
            columns: table => new
            {
                PolicyNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                PolicyVersion = table.Column<int>(type: "int", nullable: false),
                Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Exclusions", row => new { row.PolicyNumber, row.PolicyVersion, row.Code });
                table.ForeignKey("FK_Exclusions_PolicyVersions_PolicyNumber_PolicyVersion", row => new { row.PolicyNumber, row.PolicyVersion }, "PolicyVersions", new[] { "PolicyNumber", "Version" }, onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_Claims_ClaimNumber", table: "Claims", column: "ClaimNumber", unique: true);
        migrationBuilder.CreateIndex(name: "IX_ReviewQueueItems_ClaimId", table: "ReviewQueueItems", column: "ClaimId");
        migrationBuilder.CreateIndex(name: "IX_ReviewDecisionAudits_ReviewQueueItemId_CreatedAt", table: "ReviewDecisionAudits", columns: new[] { "ReviewQueueItemId", "CreatedAt" });

        migrationBuilder.InsertData("Policies", new[] { "PolicyNumber" }, new object[] { "POL-DEMO-001" });
        migrationBuilder.InsertData("PolicyVersions", new[] { "PolicyNumber", "Version", "EffectiveFrom", "EffectiveTo" }, new object[,]
        {
            { "POL-DEMO-001", 1, new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31) },
            { "POL-DEMO-001", 2, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31) }
        });
        migrationBuilder.InsertData("Coverages", new[] { "PolicyNumber", "PolicyVersion", "Code", "Name", "LimitAmount", "LimitCurrency", "DeductibleAmount", "DeductibleCurrency" }, new object[,]
        {
            { "POL-DEMO-001", 1, "WATER", "Water damage", 100000m, "EGP", 5000m, "EGP" },
            { "POL-DEMO-001", 2, "WATER", "Water damage", 250000m, "EGP", 10000m, "EGP" }
        });
        migrationBuilder.InsertData("Exclusions", new[] { "PolicyNumber", "PolicyVersion", "Code", "Description" }, new object[,]
        {
            { "POL-DEMO-001", 2, "FLOOD", "Flood damage requires adjuster review in this synthetic sample policy." }
        });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ReviewDecisionAudits");
        migrationBuilder.DropTable("Coverages");
        migrationBuilder.DropTable("Exclusions");
        migrationBuilder.DropTable("ReviewQueueItems");
        migrationBuilder.DropTable("PolicyVersions");
        migrationBuilder.DropTable("Claims");
        migrationBuilder.DropTable("Policies");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder) { }
}
