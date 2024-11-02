using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "Songs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Songs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "Playlists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Playlists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "Photos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Photos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "Genres",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Genres",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUtc",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Genres",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                computedColumnSql: "Upper([Name])",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Songs_isDeleted",
                table: "Songs",
                column: "isDeleted",
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_isDeleted",
                table: "Playlists",
                column: "isDeleted",
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_isDeleted",
                table: "Photos",
                column: "isDeleted",
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Genres_isDeleted",
                table: "Genres",
                column: "isDeleted",
                filter: "IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_isDeleted",
                table: "AspNetUsers",
                column: "isDeleted",
                filter: "IsDeleted = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Songs_isDeleted",
                table: "Songs");

            migrationBuilder.DropIndex(
                name: "IX_Playlists_isDeleted",
                table: "Playlists");

            migrationBuilder.DropIndex(
                name: "IX_Photos_isDeleted",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Genres_isDeleted",
                table: "Genres");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_isDeleted",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Genres");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Songs");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "Genres");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Genres");

            migrationBuilder.DropColumn(
                name: "DeletedOnUtc",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "AspNetUsers");
        }
    }
}
