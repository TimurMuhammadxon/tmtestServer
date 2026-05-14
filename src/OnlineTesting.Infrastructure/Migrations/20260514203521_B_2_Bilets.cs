using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class B_2_Bilets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bilets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false),
                    is_demo = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bilets", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bilet_questions",
                columns: table => new
                {
                    bilet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bilet_questions", x => new { x.bilet_id, x.question_id });
                    table.CheckConstraint("ck_bilet_questions_order_range", "order_index BETWEEN 1 AND 20");
                    table.ForeignKey(
                        name: "fk_bilet_questions_bilets_bilet_id",
                        column: x => x.bilet_id,
                        principalTable: "bilets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_bilet_questions_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ux_bilet_questions_order",
                table: "bilet_questions",
                columns: new[] { "bilet_id", "order_index" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_bilet_questions_question",
                table: "bilet_questions",
                column: "question_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_bilets_demo",
                table: "bilets",
                column: "is_demo",
                unique: true,
                filter: "is_demo = true");

            migrationBuilder.CreateIndex(
                name: "ux_bilets_number",
                table: "bilets",
                column: "number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bilet_questions");

            migrationBuilder.DropTable(
                name: "bilets");
        }
    }
}
