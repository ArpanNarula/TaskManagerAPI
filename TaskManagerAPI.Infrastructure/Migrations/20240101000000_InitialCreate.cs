using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TaskManagerAPI.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id           = table.Column<string>(type: "text", nullable: false),
                UserName     = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Email        = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                PasswordHash = table.Column<string>(type: "text", nullable: false),
                Role         = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "User"),
                CreatedAt    = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_UserName",
            table: "Users",
            column: "UserName",
            unique: true);

        migrationBuilder.CreateTable(
            name: "Tasks",
            columns: table => new
            {
                Id          = table.Column<int>(type: "integer", nullable: false)
                                  .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Title       = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                Status      = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                Priority    = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Medium"),
                CreatedAt   = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt   = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UserId      = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tasks", x => x.Id);
                table.ForeignKey(
                    name:            "FK_Tasks_Users_UserId",
                    column:          x => x.UserId,
                    principalTable:  "Users",
                    principalColumn: "Id",
                    onDelete:        ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Tasks_UserId",
            table: "Tasks",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Tasks_Status",
            table: "Tasks",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_Tasks_Priority",
            table: "Tasks",
            column: "Priority");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Tasks");
        migrationBuilder.DropTable(name: "Users");
    }
}
