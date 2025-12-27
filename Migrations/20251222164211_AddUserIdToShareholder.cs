using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaccoShareManagementSys.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToShareholder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Shareholders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Shareholders",
                keyColumn: "ShareholderId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UserId" },
                values: new object[] { new DateTime(2025, 12, 22, 8, 42, 9, 314, DateTimeKind.Local).AddTicks(6502), null });

            migrationBuilder.UpdateData(
                table: "Shareholders",
                keyColumn: "ShareholderId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UserId" },
                values: new object[] { new DateTime(2025, 12, 22, 8, 42, 9, 314, DateTimeKind.Local).AddTicks(6520), null });

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

            migrationBuilder.CreateIndex(
                name: "IX_Shareholders_UserId",
                table: "Shareholders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shareholders_AspNetUsers_UserId",
                table: "Shareholders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shareholders_AspNetUsers_UserId",
                table: "Shareholders");

            migrationBuilder.DropIndex(
                name: "IX_Shareholders_UserId",
                table: "Shareholders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Shareholders");

            migrationBuilder.UpdateData(
                table: "Shareholders",
                keyColumn: "ShareholderId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 19, 9, 21, 51, 22, DateTimeKind.Local).AddTicks(256));

            migrationBuilder.UpdateData(
                table: "Shareholders",
                keyColumn: "ShareholderId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 19, 9, 21, 51, 22, DateTimeKind.Local).AddTicks(266));

            migrationBuilder.UpdateData(
                table: "Shares",
                keyColumn: "ShareId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 19, 9, 21, 51, 22, DateTimeKind.Local).AddTicks(482));

            migrationBuilder.UpdateData(
                table: "Shares",
                keyColumn: "ShareId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 19, 9, 21, 51, 22, DateTimeKind.Local).AddTicks(492));
        }
    }
}
