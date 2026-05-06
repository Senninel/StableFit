using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StableFit.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "match_decisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    to_user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    decision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_match_decisions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "match_recommendations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    recommended_user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_match_recommendations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "matches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id_1 = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    user_id_2 = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "MatchRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EligibleUserCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchRuns", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_match_decisions_run_to",
                table: "match_decisions",
                columns: new[] { "run_id", "to_user_id" });

            migrationBuilder.CreateIndex(
                name: "ux_match_decisions_run_from_to",
                table: "match_decisions",
                columns: new[] { "run_id", "from_user_id", "to_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_match_recommendations_run_id",
                table: "match_recommendations",
                column: "run_id");

            migrationBuilder.CreateIndex(
                name: "ix_match_recommendations_run_user_rank",
                table: "match_recommendations",
                columns: new[] { "run_id", "user_id", "rank" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_match_recommendations_run_user_recommended",
                table: "match_recommendations",
                columns: new[] { "run_id", "user_id", "recommended_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_matches_status",
                table: "matches",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_matches_user_id_1",
                table: "matches",
                column: "user_id_1");

            migrationBuilder.CreateIndex(
                name: "ix_matches_user_id_2",
                table: "matches",
                column: "user_id_2");

            migrationBuilder.CreateIndex(
                name: "ux_matches_user1_user2",
                table: "matches",
                columns: new[] { "user_id_1", "user_id_2" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "match_decisions");

            migrationBuilder.DropTable(
                name: "match_recommendations");

            migrationBuilder.DropTable(
                name: "matches");

            migrationBuilder.DropTable(
                name: "MatchRuns");
        }
    }
}
