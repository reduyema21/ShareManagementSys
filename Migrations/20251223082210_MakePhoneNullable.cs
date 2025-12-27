using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaccoShareManagementSys.Migrations
{
    /// <inheritdoc />
    public partial class MakePhoneNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Shareholders",
                keyColumn: "ShareholderId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 8, 42, 9, 314, DateTimeKind.Local).AddTicks(6502));

            migrationBuilder.UpdateData(
                table: "Shareholders",
                keyColumn: "ShareholderId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 8, 42, 9, 314, DateTimeKind.Local).AddTicks(6520));

            migrationBuilder.UpdateData(
                table: "Shares",
                keyColumn: "ShareId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 8, 42, 9, 314, DateTimeKind.Local).AddTicks(6973));

            migrationBuilder.UpdateData(
                table: "Shares",
                keyColumn: "ShareId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 8, 42, 9, 314, DateTimeKind.Local).AddTicks(7006));
        }
    }
}
