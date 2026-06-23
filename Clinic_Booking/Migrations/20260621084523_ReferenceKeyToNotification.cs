using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class ReferenceKeyToNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferenceKey",
                table: "Notifications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("db3946f4-275b-4bc1-ac3a-a6e1f3f4badb"),
                column: "ConcurrencyStamp",
                value: "563332d3-51e6-4195-b510-ceb78369dfb7");

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(5386));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(5403));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(5405));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(5406));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(5408));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(5409));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(5410));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(6214));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(6219));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(6221));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(6222));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8083));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8087));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8088));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8090));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8091));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8092));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8093));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8095));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8096));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8098));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8099));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8100));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8101));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8102));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8132));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8134));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8136));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8137));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8139));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8140));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8141));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8142));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8143));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8148));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8149));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8151));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8152));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8153));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8154));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8156));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 590, DateTimeKind.Unspecified).AddTicks(3419));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 590, DateTimeKind.Unspecified).AddTicks(3436));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 590, DateTimeKind.Unspecified).AddTicks(3441));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 590, DateTimeKind.Unspecified).AddTicks(3444));

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ReferenceKey",
                table: "Notifications",
                column: "ReferenceKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notifications_ReferenceKey",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ReferenceKey",
                table: "Notifications");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("db3946f4-275b-4bc1-ac3a-a6e1f3f4badb"),
                column: "ConcurrencyStamp",
                value: "fa5d5a89-4609-4e07-a4a4-27ecb3a40fef");

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 56, DateTimeKind.Utc).AddTicks(7001));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 56, DateTimeKind.Utc).AddTicks(7005));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 56, DateTimeKind.Utc).AddTicks(7006));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 56, DateTimeKind.Utc).AddTicks(7007));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 56, DateTimeKind.Utc).AddTicks(7008));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 56, DateTimeKind.Utc).AddTicks(7008));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 56, DateTimeKind.Utc).AddTicks(7009));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 56, DateTimeKind.Utc).AddTicks(7797));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 56, DateTimeKind.Utc).AddTicks(7800));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 56, DateTimeKind.Utc).AddTicks(7801));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 56, DateTimeKind.Utc).AddTicks(7802));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7627));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7629));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7630));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7631));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7632));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7633));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7633));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7634));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7635));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7636));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7637));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7637));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7638));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7639));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7640));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7640));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7641));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7642));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7643));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7643));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7644));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7645));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7646));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7646));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7647));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7648));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7673));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7674));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7674));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 54, DateTimeKind.Utc).AddTicks(7675));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 55, DateTimeKind.Utc).AddTicks(2824));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 55, DateTimeKind.Utc).AddTicks(2831));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 55, DateTimeKind.Utc).AddTicks(2836));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 15, 14, 44, 23, 55, DateTimeKind.Utc).AddTicks(2838));
        }
    }
}
