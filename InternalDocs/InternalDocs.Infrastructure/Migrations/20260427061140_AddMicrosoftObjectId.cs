using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalDocs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMicrosoftObjectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MicrosoftObjectId",
                table: "Users",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_MicrosoftObjectId",
                table: "Users",
                column: "MicrosoftObjectId",
                unique: true,
                filter: "\"MicrosoftObjectId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_MicrosoftObjectId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MicrosoftObjectId",
                table: "Users");
        }
    }
}
