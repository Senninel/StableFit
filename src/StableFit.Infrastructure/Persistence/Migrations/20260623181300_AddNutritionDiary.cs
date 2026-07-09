using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StableFit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNutritionDiary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "biological_sex",
                table: "user_profiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "daily_calorie_target",
                table: "user_profiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "height_cm",
                table: "user_profiles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "nutrition_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    meal_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    food_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    calories = table.Column<double>(type: "double precision", nullable: false),
                    protein_grams = table.Column<double>(type: "double precision", nullable: false),
                    carb_grams = table.Column<double>(type: "double precision", nullable: false),
                    fat_grams = table.Column<double>(type: "double precision", nullable: false),
                    serving_description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nutrition_entries", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_nutrition_entries_user_date",
                table: "nutrition_entries",
                columns: new[] { "user_id", "date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "nutrition_entries");

            migrationBuilder.DropColumn(
                name: "biological_sex",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "daily_calorie_target",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "height_cm",
                table: "user_profiles");
        }
    }
}
