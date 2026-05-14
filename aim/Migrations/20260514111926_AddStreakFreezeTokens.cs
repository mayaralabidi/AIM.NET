using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aim.Migrations
{
    /// <inheritdoc />
    public partial class AddStreakFreezeTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StreakFreezeTokens",
                table: "Participants",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StreakFreezeTokens",
                table: "Participants");
        }
    }
}
