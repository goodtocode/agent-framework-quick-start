using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateAgentFrameworkContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Chat");

            migrationBuilder.CreateTable(
                name: "Actors",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "NVARCHAR(200)", nullable: true),
                    LastName = table.Column<string>(type: "NVARCHAR(200)", nullable: true),
                    Email = table.Column<string>(type: "NVARCHAR(200)", nullable: true),
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
                    table.PrimaryKey("PK_Actors", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ChatGovernance",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PolicyProfileVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TraceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrincipalDisplay = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ModelRef = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PromptHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    InputHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DeterministicReplaySupported = table.Column<bool>(type: "bit", nullable: false),
                    SystemInstruction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvidenceRefsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToolRefsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PoliciesAppliedJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JustificationRefsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReasoningSummary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RowKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatGovernance", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ChatSessions",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonaVersion = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatSessions", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatSessions_ChatSessionId",
                        column: x => x.ChatSessionId,
                        principalSchema: "Chat",
                        principalTable: "ChatSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actors_TenantId_OwnerId",
                schema: "Chat",
                table: "Actors",
                columns: new[] { "TenantId", "OwnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Actors_Timestamp",
                schema: "Chat",
                table: "Actors",
                column: "Timestamp",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatGovernance_TenantId_OwnerId_ChatSessionId",
                schema: "Chat",
                table: "ChatGovernance",
                columns: new[] { "TenantId", "OwnerId", "ChatSessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatGovernance_Timestamp",
                schema: "Chat",
                table: "ChatGovernance",
                column: "Timestamp",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ChatSessionId",
                schema: "Chat",
                table: "ChatMessages",
                column: "ChatSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_Timestamp",
                schema: "Chat",
                table: "ChatMessages",
                column: "Timestamp",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatSessions_Timestamp",
                schema: "Chat",
                table: "ChatSessions",
                column: "Timestamp",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Actors",
                schema: "Chat");

            migrationBuilder.DropTable(
                name: "ChatGovernance",
                schema: "Chat");

            migrationBuilder.DropTable(
                name: "ChatMessages",
                schema: "Chat");

            migrationBuilder.DropTable(
                name: "ChatSessions",
                schema: "Chat");
        }
    }
}
