using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalDocs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentSemanticVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MajorVersion",
                table: "DocumentVersions",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "MinorVersion",
                table: "DocumentVersions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MajorVersion",
                table: "DocumentVersions");

            migrationBuilder.DropColumn(
                name: "MinorVersion",
                table: "DocumentVersions");
        }
    }
}
