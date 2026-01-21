using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaccoShareManagementSys.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToProfileImg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ProfileImg",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Shareholders",
                keyColumn: "ShareholderId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 30, 21, 50, 48, 85, DateTimeKind.Local).AddTicks(3299));

            migrationBuilder.UpdateData(
                table: "Shareholders",
                keyColumn: "ShareholderId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 30, 21, 50, 48, 85, DateTimeKind.Local).AddTicks(3317));

            migrationBuilder.UpdateData(
                table: "Shares",
                keyColumn: "ShareId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 30, 21, 50, 48, 85, DateTimeKind.Local).AddTicks(3669));

            migrationBuilder.UpdateData(
                table: "Shares",
                keyColumn: "ShareId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 30, 21, 50, 48, 85, DateTimeKind.Local).AddTicks(3686));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ProfileImg");

            migrationBuilder.UpdateData(
                table: "Shareholders",
                keyColumn: "ShareholderId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 24, 22, 41, 32, 615, DateTimeKind.Local).AddTicks(8069));

            migrationBuilder.UpdateData(
                table: "Shareholders",
                keyColumn: "ShareholderId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 24, 22, 41, 32, 615, DateTimeKind.Local).AddTicks(8087));

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
    }
}
