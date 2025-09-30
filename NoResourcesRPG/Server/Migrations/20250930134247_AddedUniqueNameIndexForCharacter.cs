using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoResourcesRPG.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddedUniqueNameIndexForCharacter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Characters_Name",
                table: "Characters",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Characters_Name",
                table: "Characters");
        }
    }
}
