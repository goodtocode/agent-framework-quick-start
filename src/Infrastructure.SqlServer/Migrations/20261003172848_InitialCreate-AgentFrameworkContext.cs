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
                name: "IntentEmbeddings",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IntentName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false, collation: "SQL_Latin1_General_CP1_CI_AS"),
                    Source = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SourceText = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    Vector = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    Weight = table.Column<float>(type: "REAL", nullable: false, defaultValue: 1f),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntentEmbeddings", x => x.Id);
                });

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
                name: "RequestIdempotency",
                schema: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OperationKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    RequestHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ResponseType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ResponsePayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResourceType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScopeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.PrimaryKey("PK_RequestIdempotency", x => x.Id)
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
                    EvidenceJson = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
                    FindingJson = table.Column<string>(type: "NVARCHAR(MAX)", nullable: true),
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

            migrationBuilder.CreateIndex(
                name: "IX_IntentEmbeddings_IntentName",
                schema: "Chat",
                table: "IntentEmbeddings",
                column: "IntentName");

            migrationBuilder.CreateIndex(
                name: "IX_IntentEmbeddings_IntentName_Source",
                schema: "Chat",
                table: "IntentEmbeddings",
                columns: new[] { "IntentName", "Source" });

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

            migrationBuilder.CreateIndex(
                name: "IX_RequestIdempotency_DuplicateWindowLookup",
                schema: "Chat",
                table: "RequestIdempotency",
                columns: new[] { "TenantId", "OwnerId", "OperationKey", "ScopeId", "RequestHash", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestIdempotency_TenantOwnerOperationKey",
                schema: "Chat",
                table: "RequestIdempotency",
                columns: new[] { "TenantId", "OwnerId", "OperationKey", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestIdempotency_Timestamp",
                schema: "Chat",
                table: "RequestIdempotency",
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
                name: "IntentEmbeddings",
                schema: "Chat");

            migrationBuilder.DropTable(
                name: "PlaybookExecutions",
                schema: "Chat");

            migrationBuilder.DropTable(
                name: "PlaybookSteps",
                schema: "Chat");

            migrationBuilder.DropTable(
                name: "RequestIdempotency",
                schema: "Chat");

            migrationBuilder.DropTable(
                name: "ChatSessions",
                schema: "Chat");

            migrationBuilder.DropTable(
                name: "Playbooks",
                schema: "Chat");
        }
    }
}
