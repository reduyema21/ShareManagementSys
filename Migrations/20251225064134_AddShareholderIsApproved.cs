using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaccoShareManagementSys.Migrations
{
    /// <inheritdoc />
    public partial class AddShareholderIsApproved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Shareholders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Shareholders",
                keyColumn: "ShareholderId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "IsApproved" },
                values: new object[] { new DateTime(2025, 12, 24, 22, 41, 32, 615, DateTimeKind.Local).AddTicks(8069), false });

            migrationBuilder.UpdateData(
                table: "Shareholders",
                keyColumn: "ShareholderId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "IsApproved" },
                values: new object[] { new DateTime(2025, 12, 24, 22, 41, 32, 615, DateTimeKind.Local).AddTicks(8087), false });

            migrationBuilder.UpdateData(
                table: "Shares",
                keyColumn: "ShareId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 24, 22, 41, 32, 615, DateTimeKind.Local).AddTicks(8498));

            migrationBuilder.UpdateData(
                table: "Shares",
                keyColumn: "ShareId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 24, 22, 41, 32, 615, DateTimeKind.Local).AddTicks(8514));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Shareholders");

            migrationBuilder.UpdateData(
                table: "Shareholders",
                keyColumn: "ShareholderId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 0, 22, 8, 707, DateTimeKind.Local).AddTicks(8367));

            migrationBuilder.UpdateData(
                table: "Shareholders",
                keyColumn: "ShareholderId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 0, 22, 8, 707, DateTimeKind.Local).AddTicks(8378));

            migrationBuilder.UpdateData(
                table: "Shares",
                keyColumn: "ShareId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 0, 22, 8, 707, DateTimeKind.Local).AddTicks(8600));

            migrationBuilder.UpdateData(
                table: "Shares",
                keyColumn: "ShareId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 0, 22, 8, 707, DateTimeKind.Local).AddTicks(8610));
        }
    }
}
