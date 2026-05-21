using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class B_7_3_ClickIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "click_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    prepare_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    click_transaction_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    prepare_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    complete_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_click_transactions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_click_transactions_order_id",
                table: "click_transactions",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ux_click_transactions_click_id",
                table: "click_transactions",
                column: "click_transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_click_transactions_prepare_id",
                table: "click_transactions",
                column: "prepare_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "click_transactions");
        }
    }
}
