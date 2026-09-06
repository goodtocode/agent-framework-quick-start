using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Goodtocode.AgentFramework.Infrastructure.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddIntentEmbeddingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntentEmbeddings",
                schema: "Chat");
        }
    }
}
