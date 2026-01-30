using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace music_manager_starter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedUserToPlaylist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UpdatedByUserId",
                table: "Playlists",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedByUserId",
                table: "Playlists");
        }
    }
}
