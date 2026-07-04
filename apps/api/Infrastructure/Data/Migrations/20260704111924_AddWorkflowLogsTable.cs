using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrowserAgent.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workflow_logs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Level = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StepName = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: true),
                    ScreenshotPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_logs_workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalSchema: "public",
                        principalTable: "workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_Timestamp",
                schema: "public",
                table: "workflow_logs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_logs_WorkflowId",
                schema: "public",
                table: "workflow_logs",
                column: "WorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workflow_logs",
                schema: "public");
        }
    }
}
