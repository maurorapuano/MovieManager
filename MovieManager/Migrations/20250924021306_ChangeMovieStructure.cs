using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MovieManager.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMovieStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReleaseDate",
                table: "Movies",
                newName: "ReleaseYear");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReleaseYear",
                table: "Movies",
                newName: "ReleaseDate");
        }
    }
}
