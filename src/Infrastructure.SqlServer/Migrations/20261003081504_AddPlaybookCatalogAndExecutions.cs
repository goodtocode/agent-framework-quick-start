using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaybookCatalogAndExecutions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaybookMaterializations",
                schema: "Chat");

            migrationBuilder.CreateTable(
                name: "Playbooks",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "NVARCHAR(200)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR(200)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR(1000)", nullable: false),
                    WorkflowType = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    Version = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    RowKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playbooks", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "PlaybookExecutions",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlaybookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlaybookKey = table.Column<string>(type: "NVARCHAR(200)", nullable: false),
                    PlaybookVersion = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    WorkflowType = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    ReplayMode = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    SourceExecutionId = table.Column<string>(type: "NVARCHAR(200)", nullable: true),
                    CollectInput = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    CollectOutput = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    EvaluateOutput = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    RecordOutput = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    StartedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RowKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OwnerId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    TenantId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybookExecutions", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PlaybookExecutions_Playbooks_PlaybookId",
                        column: x => x.PlaybookId,
                        principalSchema: "Chat",
                        principalTable: "Playbooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaybookSteps",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlaybookId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StepType = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR(200)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR(1000)", nullable: false),
                    ActionFormat = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    ActionDefinition = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    RowKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybookSteps", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PlaybookSteps_Playbooks_PlaybookId",
                        column: x => x.PlaybookId,
                        principalSchema: "Chat",
                        principalTable: "Playbooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookExecutions_PlaybookId",
                schema: "Chat",
                table: "PlaybookExecutions",
                column: "PlaybookId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookExecutions_TenantId_OwnerId_PlaybookKey",
                schema: "Chat",
                table: "PlaybookExecutions",
                columns: new[] { "TenantId", "OwnerId", "PlaybookKey" });

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookExecutions_Timestamp",
                schema: "Chat",
                table: "PlaybookExecutions",
                column: "Timestamp",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Playbooks_Key",
                schema: "Chat",
                table: "Playbooks",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Playbooks_Timestamp",
                schema: "Chat",
                table: "Playbooks",
                column: "Timestamp",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookSteps_PlaybookId_StepType",
                schema: "Chat",
                table: "PlaybookSteps",
                columns: new[] { "PlaybookId", "StepType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookSteps_Timestamp",
                schema: "Chat",
                table: "PlaybookSteps",
                column: "Timestamp",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaybookExecutions",
                schema: "Chat");

            migrationBuilder.DropTable(
                name: "PlaybookSteps",
                schema: "Chat");

            migrationBuilder.DropTable(
                name: "Playbooks",
                schema: "Chat");

            migrationBuilder.CreateTable(
                name: "PlaybookMaterializations",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    PayloadSnapshot = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    PlaybookKey = table.Column<string>(type: "NVARCHAR(200)", nullable: false),
                    PlaybookVersion = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    RowKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SummaryText = table.Column<string>(type: "NVARCHAR(1000)", nullable: false),
                    TenantId = table.Column<Guid>(type: "UNIQUEIDENTIFIER", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    WorkflowType = table.Column<string>(type: "NVARCHAR(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybookMaterializations", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookMaterializations_TenantId_OwnerId_PlaybookKey",
                schema: "Chat",
                table: "PlaybookMaterializations",
                columns: new[] { "TenantId", "OwnerId", "PlaybookKey" });

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookMaterializations_Timestamp",
                schema: "Chat",
                table: "PlaybookMaterializations",
                column: "Timestamp",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }
    }
}
