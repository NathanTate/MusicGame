using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedJoinTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSong_Playlists_PlaylistId",
                table: "PlaylistSong");

            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSong_Songs_SongId",
                table: "PlaylistSong");

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "Songs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "PlaylistSong",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistSong_Playlists_SongId",
                table: "PlaylistSong",
                column: "SongId",
                principalTable: "Playlists",
                principalColumn: "PlaylistId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistSong_Songs_PlaylistId",
                table: "PlaylistSong",
                column: "PlaylistId",
                principalTable: "Songs",
                principalColumn: "SongId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSong_Playlists_SongId",
                table: "PlaylistSong");

            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSong_Songs_PlaylistId",
                table: "PlaylistSong");

            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "PlaylistSong");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistSong_Playlists_PlaylistId",
                table: "PlaylistSong",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "PlaylistId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistSong_Songs_SongId",
                table: "PlaylistSong",
                column: "SongId",
                principalTable: "Songs",
                principalColumn: "SongId");
        }
    }
}
