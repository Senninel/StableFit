using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StableFit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredGymToUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "preferred_gym_address",
                table: "user_profiles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "preferred_gym_latitude",
                table: "user_profiles",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "preferred_gym_longitude",
                table: "user_profiles",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preferred_gym_name",
                table: "user_profiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "preferred_gym_address",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "preferred_gym_latitude",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "preferred_gym_longitude",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "preferred_gym_name",
                table: "user_profiles");
        }
    }
}
