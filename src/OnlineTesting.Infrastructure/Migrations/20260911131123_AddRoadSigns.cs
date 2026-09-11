using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoadSigns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "road_sign_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_road_sign_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "road_sign_category_translations",
                columns: table => new
                {
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_road_sign_category_translations", x => new { x.category_id, x.language_code });
                    table.ForeignKey(
                        name: "fk_road_sign_category_translations_languages_language_code",
                        column: x => x.language_code,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_road_sign_category_translations_road_sign_categories_catego",
                        column: x => x.category_id,
                        principalTable: "road_sign_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "road_signs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    image_key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_road_signs", x => x.id);
                    table.ForeignKey(
                        name: "fk_road_signs_road_sign_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "road_sign_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "road_sign_translations",
                columns: table => new
                {
                    sign_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_road_sign_translations", x => new { x.sign_id, x.language_code });
                    table.ForeignKey(
                        name: "fk_road_sign_translations_languages_language_code",
                        column: x => x.language_code,
                        principalTable: "languages",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_road_sign_translations_road_signs_sign_id",
                        column: x => x.sign_id,
                        principalTable: "road_signs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_road_sign_category_translations_language_code",
                table: "road_sign_category_translations",
                column: "language_code");

            migrationBuilder.CreateIndex(
                name: "ix_road_sign_translations_language_code",
                table: "road_sign_translations",
                column: "language_code");

            migrationBuilder.CreateIndex(
                name: "ix_road_signs_category_id",
                table: "road_signs",
                column: "category_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "road_sign_category_translations");

            migrationBuilder.DropTable(
                name: "road_sign_translations");

            migrationBuilder.DropTable(
                name: "road_signs");

            migrationBuilder.DropTable(
                name: "road_sign_categories");
        }
    }
}
