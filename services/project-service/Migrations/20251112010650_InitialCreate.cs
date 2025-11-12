using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace project_service.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "spm_project");

            migrationBuilder.CreateTable(
                name: "projects",
                schema: "spm_project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "project_members",
                schema: "spm_project",
                columns: table => new
                {
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_members", x => new { x.ProjectId, x.UserId });
                    table.ForeignKey(
                        name: "FK_project_members_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "spm_project",
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tasks",
                schema: "spm_project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AssignedTo = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Priority = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tasks_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "spm_project",
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comments",
                schema: "spm_project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comments_tasks_TaskId",
                        column: x => x.TaskId,
                        principalSchema: "spm_project",
                        principalTable: "tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_comments_created_at",
                schema: "spm_project",
                table: "comments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "idx_comments_task_id",
                schema: "spm_project",
                table: "comments",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "idx_comments_user_id",
                schema: "spm_project",
                table: "comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "idx_project_members_role",
                schema: "spm_project",
                table: "project_members",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "idx_project_members_user_id",
                schema: "spm_project",
                table: "project_members",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "idx_projects_created_at",
                schema: "spm_project",
                table: "projects",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "idx_projects_created_by",
                schema: "spm_project",
                table: "projects",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "idx_projects_is_active",
                schema: "spm_project",
                table: "projects",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "idx_tasks_assigned_to",
                schema: "spm_project",
                table: "tasks",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "idx_tasks_created_by",
                schema: "spm_project",
                table: "tasks",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "idx_tasks_due_date",
                schema: "spm_project",
                table: "tasks",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "idx_tasks_priority",
                schema: "spm_project",
                table: "tasks",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "idx_tasks_project_id",
                schema: "spm_project",
                table: "tasks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "idx_tasks_status",
                schema: "spm_project",
                table: "tasks",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comments",
                schema: "spm_project");

            migrationBuilder.DropTable(
                name: "project_members",
                schema: "spm_project");

            migrationBuilder.DropTable(
                name: "tasks",
                schema: "spm_project");

            migrationBuilder.DropTable(
                name: "projects",
                schema: "spm_project");
        }
    }
}
