using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class B_7_1_Subscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "subscription_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    duration = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscription_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscriptions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_subscription_plans_type_duration",
                table: "subscription_plans",
                columns: new[] { "type", "duration" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_expires_at",
                table: "subscriptions",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ux_subscriptions_user_id",
                table: "subscriptions",
                column: "user_id",
                unique: true);

            // Seed 8 subscription plans: Student(1) + Teacher(2) x 4 durations
            // type: Student=1, Teacher=2 | duration: TwoWeeks=1, OneMonth=2, TwoMonths=3, ThreeMonths=4
            migrationBuilder.InsertData(
                table: "subscription_plans",
                columns: new[] { "id", "type", "duration", "price", "is_active" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-0000-0000-000000000001"), 1, 1, 0m, true },
                    { new Guid("a1000000-0000-0000-0000-000000000002"), 1, 2, 0m, true },
                    { new Guid("a1000000-0000-0000-0000-000000000003"), 1, 3, 0m, true },
                    { new Guid("a1000000-0000-0000-0000-000000000004"), 1, 4, 0m, true },
                    { new Guid("a1000000-0000-0000-0000-000000000005"), 2, 1, 0m, true },
                    { new Guid("a1000000-0000-0000-0000-000000000006"), 2, 2, 0m, true },
                    { new Guid("a1000000-0000-0000-0000-000000000007"), 2, 3, 0m, true },
                    { new Guid("a1000000-0000-0000-0000-000000000008"), 2, 4, 0m, true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "subscription_plans");
            migrationBuilder.DropTable(name: "subscriptions");
        }
    }
}
