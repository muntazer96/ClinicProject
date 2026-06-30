using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("db3946f4-275b-4bc1-ac3a-a6e1f3f4badb"),
                column: "ConcurrencyStamp",
                value: "010b1701-37ed-4cdc-b7a0-70542219dcd9");

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 939, DateTimeKind.Unspecified).AddTicks(915));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 939, DateTimeKind.Unspecified).AddTicks(933));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 939, DateTimeKind.Unspecified).AddTicks(935));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 939, DateTimeKind.Unspecified).AddTicks(936));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 939, DateTimeKind.Unspecified).AddTicks(937));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 939, DateTimeKind.Unspecified).AddTicks(938));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 939, DateTimeKind.Unspecified).AddTicks(975));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 939, DateTimeKind.Unspecified).AddTicks(1917));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 939, DateTimeKind.Unspecified).AddTicks(1922));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 939, DateTimeKind.Unspecified).AddTicks(1923));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 939, DateTimeKind.Unspecified).AddTicks(1925));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1586));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1590));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1591));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1592));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1594));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1595));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1597));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1598));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1599));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1600));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1601));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1602));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1603));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1605));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1606));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1607));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1608));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1609));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1610));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1611));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1612));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1613));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1614));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1615));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1616));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1617));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1619));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1620));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1621));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1622));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1623));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1624));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1625));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1626));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1627));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1628));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1629));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1630));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1632));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1633));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1634));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1635));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1636));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(1681));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(7991));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(8015));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(8021));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 12, 40, 937, DateTimeKind.Unspecified).AddTicks(8023));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("db3946f4-275b-4bc1-ac3a-a6e1f3f4badb"),
                column: "ConcurrencyStamp",
                value: "097484ee-36cb-486b-8135-7e7fa1d027c7");

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(911));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(928));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(930));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(931));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(932));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(933));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(934));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(1877));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(1916));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(1918));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(1919));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(227));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(231));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(233));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(234));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(235));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(237));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(238));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(239));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(240));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(242));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(243));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(244));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(245));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(246));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(247));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(285));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(287));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(288));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(289));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(291));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(292));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(293));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(294));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(295));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(296));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(298));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(299));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(300));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(301));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(302));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(303));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(304));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(305));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(307));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(308));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(309));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(310));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(311));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(312));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(314));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(315));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(316));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(317));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(318));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(6622));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(6638));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(6644));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(6647));
        }
    }
}
