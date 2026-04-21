using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StableFit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_profiles_display_name",
                table: "user_profiles");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "user_profiles",
                newName: "name");

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "user_profiles",
                type: "character varying(320)",
                maxLength: 320,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "user_profiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ux_user_profiles_email",
                table: "user_profiles",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_user_profiles_username",
                table: "user_profiles",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_user_profiles_email",
                table: "user_profiles");

            migrationBuilder.DropIndex(
                name: "ux_user_profiles_username",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "email",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "username",
                table: "user_profiles");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "user_profiles",
                newName: "display_name");

            migrationBuilder.CreateIndex(
                name: "ix_user_profiles_display_name",
                table: "user_profiles",
                column: "display_name");
        }
    }
}
