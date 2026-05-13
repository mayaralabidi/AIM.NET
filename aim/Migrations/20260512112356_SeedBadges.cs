using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace aim.Migrations
{
    /// <inheritdoc />
    public partial class SeedBadges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Badges",
                columns: new[] { "Id", "Criteria", "IconPath", "Key", "Name" },
                values: new object[,]
                {
                    { 1, "3 day streak", "3days.png", "streak_3", "First Steps" },
                    { 2, "7 day streak", "7days.png", "streak_7", "Week Warrior" },
                    { 3, "14 day streak", "14days.png", "streak_14", "Fortnight Fighter" },
                    { 4, "30 day streak", "30days.png", "streak_30", "Iron Will" },
                    { 5, "100 day streak", "100days.png", "streak_100", "Legendary" },
                    { 6, "Complete first task", "1stblood.png", "tasks_1", "First Blood" },
                    { 7, "Complete 10 tasks", "10tasks.png", "tasks_10", "Warming Up" },
                    { 8, "Complete 50 tasks", "50tasks.png", "tasks_50", "Half Century" },
                    { 9, "Complete 100 tasks", "100tasks.png", "tasks_100", "Centurion" },
                    { 10, "Complete 250 tasks", "250tasks.png", "tasks_250", "Machine" },
                    { 11, "Complete 500 tasks", "500tasks.png", "tasks_500", "Relentless" },
                    { 12, "Reach 500 points", "500score.png", "score_500", "Rising Star" },
                    { 13, "Reach 1000 points", "1000score.png", "score_1000", "High Roller" },
                    { 14, "Reach 5000 points", "5000score.png", "score_5000", "Elite" },
                    { 15, "Reach 10000 points", "10000score.png", "score_10000", "Untouchable" },
                    { 16, "Join a team", "joinateam.png", "team_join", "Squad Up" },
                    { 17, "Reach top 3 in your team", "podium.png", "team_top3", "Podium" },
                    { 18, "Reach #1 in your team", "alpha.png", "team_top1", "Alpha" },
                    { 19, "Restart a streak after losing one", "comebackkid.png", "comeback", "Comeback Kid" },
                    { 20, "Complete a task before 8am", "earlybird.png", "early_bird", "Early Bird" },
                    { 21, "Complete a task after 10pm", "nightowl.png", "night_owl", "Night Owl" },
                    { 22, "Complete every task in a challenge", "overachiever.png", "overachiever", "Overachiever" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 22);
        }
    }
}
