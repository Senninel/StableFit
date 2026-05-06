using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StableFit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixMatchRecommendationIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "ix_match_recommendations_run_user_rank",
                table: "match_recommendations",
                newName: "ux_match_recommendations_run_user_rank");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "ux_match_recommendations_run_user_rank",
                table: "match_recommendations",
                newName: "ix_match_recommendations_run_user_rank");
        }
    }
}
