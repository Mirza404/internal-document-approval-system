using System;
using InternalDocs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalDocs.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260506073600_AddDocumentCategories")]
    public partial class AddDocumentCategories : Migration
    {
        private static readonly Guid HrCategoryId = new("11111111-1111-1111-1111-111111111111");
        private static readonly Guid FinanceCategoryId = new("22222222-2222-2222-2222-222222222222");
        private static readonly Guid ContractCategoryId = new("33333333-3333-3333-3333-333333333333");
        private static readonly Guid GenericCategoryId = new("44444444-4444-4444-4444-444444444444");
        private static readonly string[] CategorySeedColumns = ["Id", "Name", "Description", "CreatedAt"];
        private static readonly string[] CategorySeedColumnTypes = ["uuid", "character varying(128)", "character varying(500)", "timestamp with time zone"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentCategories", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "DocumentCategories",
                columns: CategorySeedColumns,
                columnTypes: CategorySeedColumnTypes,
                values: new object[,]
                {
                    { HrCategoryId, "HR", "People operations, onboarding, leave, and employment documents.", new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc) },
                    { FinanceCategoryId, "Finance", "Budgets, invoices, reimbursements, procurement, and financial approvals.", new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc) },
                    { ContractCategoryId, "Contract", "Vendor, partner, service, and legal agreement documents.", new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc) },
                    { GenericCategoryId, "Generic", "General internal documents that do not need a specialized category.", new DateTime(2026, 5, 6, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "DocumentTypes",
                type: "uuid",
                nullable: false,
                defaultValue: GenericCategoryId);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_CategoryId",
                table: "DocumentTypes",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentCategories_Name",
                table: "DocumentCategories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTypes_DocumentCategories_CategoryId",
                table: "DocumentTypes",
                column: "CategoryId",
                principalTable: "DocumentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTypes_DocumentCategories_CategoryId",
                table: "DocumentTypes");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTypes_CategoryId",
                table: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "DocumentCategories");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "DocumentTypes");
        }
    }
}
