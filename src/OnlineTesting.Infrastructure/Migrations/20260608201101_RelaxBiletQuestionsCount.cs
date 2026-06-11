using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RelaxBiletQuestionsCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_bilet_questions_order_range",
                table: "bilet_questions");

            migrationBuilder.AddCheckConstraint(
                name: "ck_bilet_questions_order_range",
                table: "bilet_questions",
                sql: "order_index >= 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_bilet_questions_order_range",
                table: "bilet_questions");

            migrationBuilder.AddCheckConstraint(
                name: "ck_bilet_questions_order_range",
                table: "bilet_questions",
                sql: "order_index BETWEEN 1 AND 20");
        }
    }
}
