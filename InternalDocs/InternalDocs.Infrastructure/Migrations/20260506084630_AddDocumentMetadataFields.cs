using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternalDocs.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentMetadataFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "Documents",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentNote",
                table: "Documents",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BudgetCode",
                table: "Documents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Counterparty",
                table: "Documents",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LeaveEndDate",
                table: "Documents",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LeaveStartDate",
                table: "Documents",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeaveType",
                table: "Documents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "AttachmentNote",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "BudgetCode",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "Counterparty",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "LeaveEndDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "LeaveStartDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "LeaveType",
                table: "Documents");
        }
    }
}
