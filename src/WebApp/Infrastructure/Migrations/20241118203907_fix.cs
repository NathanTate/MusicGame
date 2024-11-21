using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSong_Playlists_SongId",
                table: "PlaylistSong");

            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSong_Songs_PlaylistId",
                table: "PlaylistSong");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlaylistSong",
                table: "PlaylistSong");

            migrationBuilder.RenameTable(
                name: "PlaylistSong",
                newName: "PlaylistSongs");

            migrationBuilder.RenameIndex(
                name: "IX_PlaylistSong_SongId",
                table: "PlaylistSongs",
                newName: "IX_PlaylistSongs_SongId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlaylistSongs",
                table: "PlaylistSongs",
                columns: new[] { "PlaylistId", "SongId" });

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistSongs_Playlists_SongId",
                table: "PlaylistSongs",
                column: "SongId",
                principalTable: "Playlists",
                principalColumn: "PlaylistId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaylistSongs_Songs_PlaylistId",
                table: "PlaylistSongs",
                column: "PlaylistId",
                principalTable: "Songs",
                principalColumn: "SongId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSongs_Playlists_SongId",
                table: "PlaylistSongs");

            migrationBuilder.DropForeignKey(
                name: "FK_PlaylistSongs_Songs_PlaylistId",
                table: "PlaylistSongs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlaylistSongs",
                table: "PlaylistSongs");

            migrationBuilder.RenameTable(
                name: "PlaylistSongs",
                newName: "PlaylistSong");

            migrationBuilder.RenameIndex(
                name: "IX_PlaylistSongs_SongId",
                table: "PlaylistSong",
                newName: "IX_PlaylistSong_SongId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlaylistSong",
                table: "PlaylistSong",
                columns: new[] { "PlaylistId", "SongId" });

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
    }
}
