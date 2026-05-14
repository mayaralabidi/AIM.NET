using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aim.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarConfig",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstagramHandle",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwitterHandle",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarConfig",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "InstagramHandle",
                table: "Participants");

            migrationBuilder.DropColumn(
                name: "TwitterHandle",
                table: "Participants");
        }
    }
}
