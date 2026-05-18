using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class B_6_2_TestLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "test_link_id",
                table: "attempts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "test_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    flow_type = table.Column<int>(type: "integer", nullable: false),
                    bilet_id = table.Column<Guid>(type: "uuid", nullable: true),
                    topic_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    question_count = table.Column<int>(type: "integer", nullable: true),
                    group_id = table.Column<Guid>(type: "uuid", nullable: true),
                    max_attempts = table.Column<int>(type: "integer", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_test_links", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_test_links_teacher_id",
                table: "test_links",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "ux_test_links_code",
                table: "test_links",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "test_links");

            migrationBuilder.DropColumn(
                name: "test_link_id",
                table: "attempts");
        }
    }
}
