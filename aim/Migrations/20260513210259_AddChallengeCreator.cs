using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aim.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengeCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "Challenges",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByParticipantId",
                table: "Challenges",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Challenges_CreatedById",
                table: "Challenges",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Challenges_Participants_CreatedById",
                table: "Challenges",
                column: "CreatedById",
                principalTable: "Participants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Challenges_Participants_CreatedById",
                table: "Challenges");

            migrationBuilder.DropIndex(
                name: "IX_Challenges_CreatedById",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "CreatedByParticipantId",
                table: "Challenges");
        }
    }
}
