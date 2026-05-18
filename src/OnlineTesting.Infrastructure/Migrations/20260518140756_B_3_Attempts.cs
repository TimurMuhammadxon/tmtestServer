using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class B_3_Attempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    flow_type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    correct_count = table.Column<int>(type: "integer", nullable: true),
                    bilet_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attempts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "attempt_questions",
                columns: table => new
                {
                    attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    chosen_answer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_correct = table.Column<bool>(type: "boolean", nullable: true),
                    answered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attempt_questions", x => new { x.attempt_id, x.question_id });
                    table.ForeignKey(
                        name: "fk_attempt_questions_attempts_attempt_id",
                        column: x => x.attempt_id,
                        principalTable: "attempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_attempt_questions_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_attempt_questions_question_id",
                table: "attempt_questions",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "ix_attempts_user_id",
                table: "attempts",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attempt_questions");

            migrationBuilder.DropTable(
                name: "attempts");
        }
    }
}
