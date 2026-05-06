using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StableFit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MatchRuns",
                table: "MatchRuns");

            migrationBuilder.RenameTable(
                name: "MatchRuns",
                newName: "match_runs");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "match_runs",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ExpiresAtUtc",
                table: "match_runs",
                newName: "expires_at_utc");

            migrationBuilder.RenameColumn(
                name: "EligibleUserCount",
                table: "match_runs",
                newName: "eligible_user_count");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "match_runs",
                newName: "created_at_utc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_match_runs",
                table: "match_runs",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "ix_match_runs_expires_at_utc",
                table: "match_runs",
                column: "expires_at_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_match_runs",
                table: "match_runs");

            migrationBuilder.DropIndex(
                name: "ix_match_runs_expires_at_utc",
                table: "match_runs");

            migrationBuilder.RenameTable(
                name: "match_runs",
                newName: "MatchRuns");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "MatchRuns",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "expires_at_utc",
                table: "MatchRuns",
                newName: "ExpiresAtUtc");

            migrationBuilder.RenameColumn(
                name: "eligible_user_count",
                table: "MatchRuns",
                newName: "EligibleUserCount");

            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                table: "MatchRuns",
                newName: "CreatedAtUtc");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MatchRuns",
                table: "MatchRuns",
                column: "Id");
        }
    }
}
