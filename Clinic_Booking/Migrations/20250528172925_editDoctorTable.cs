using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class editDoctorTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("28dd0b27-8929-4d5c-956d-70537bd756d3"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bb3f3792-b4e8-460f-a977-3d95029cbb6c"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bf56622f-c2b0-4228-8d12-04268f738630"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d6f236d9-179c-4993-bf7e-3cd3d6b1bf64"));

            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDay",
                table: "Doctors",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "ImageName",
                table: "Doctors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IraqiProvince",
                table: "Doctors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("52f400aa-1273-4afe-b2ed-2a5dd9eac6ee"), null, new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2022), null, null, null, false, null, null, "DoctorUser", "DOCTORUSER" },
                    { new Guid("545c2163-4c61-4598-ae91-c94530c30e38"), null, new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2020), null, null, null, false, null, null, "NormalUser", "NORMALUSER" },
                    { new Guid("c9327af8-404c-4702-92de-1315f4749ed3"), null, new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(1997), null, null, null, false, null, null, "SuperAdmin", "SUPERADMIN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Email", "EmailConfirmed", "ImageName", "IsDeleted", "IsFirstLogin", "IsLocked", "LastLoginDate", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifierId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("85f95157-4e6b-481b-8e63-4c1b837d942a"), 0, "3e3a8c80-8472-494b-a867-95d9d76b1723", new DateTime(2025, 5, 28, 17, 29, 23, 803, DateTimeKind.Utc).AddTicks(9388), null, null, null, null, false, null, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, null, null, null, null, "SUPARADMIN", "AQAAAAIAAYagAAAAEPOWIFtSTE9UZwJZyKuLGXPHTqqYS/z06O6//rT0EgrSzn2YTFjDGVhZ/mgwj+VWSA==", null, false, "f334bc41-7e34-47ef-a008-36f52ea08772", false, "superadmin" });

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 862, DateTimeKind.Local).AddTicks(4051));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 862, DateTimeKind.Local).AddTicks(4057));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 862, DateTimeKind.Local).AddTicks(4058));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 862, DateTimeKind.Local).AddTicks(4059));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 862, DateTimeKind.Local).AddTicks(4060));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 862, DateTimeKind.Local).AddTicks(4061));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 862, DateTimeKind.Local).AddTicks(4061));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2437));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2440));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2441));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2442));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2443));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2444));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2444));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2445));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2447));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2448));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2449));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2450));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2451));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2452));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2452));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2453));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2454));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2455));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2455));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2456));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2457));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2458));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2458));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2459));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2460));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2461));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2461));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2462));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2463));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 860, DateTimeKind.Local).AddTicks(2498));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 861, DateTimeKind.Local).AddTicks(1661));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 861, DateTimeKind.Local).AddTicks(1672));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 861, DateTimeKind.Local).AddTicks(1676));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 29, 23, 861, DateTimeKind.Local).AddTicks(1678));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("52f400aa-1273-4afe-b2ed-2a5dd9eac6ee"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("545c2163-4c61-4598-ae91-c94530c30e38"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("c9327af8-404c-4702-92de-1315f4749ed3"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("85f95157-4e6b-481b-8e63-4c1b837d942a"));

            migrationBuilder.DropColumn(
                name: "BirthDay",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ImageName",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "IraqiProvince",
                table: "Doctors");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("28dd0b27-8929-4d5c-956d-70537bd756d3"), null, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2294), null, null, null, false, null, null, "NormalUser", "NORMALUSER" },
                    { new Guid("bb3f3792-b4e8-460f-a977-3d95029cbb6c"), null, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2296), null, null, null, false, null, null, "DoctorUser", "DOCTORUSER" },
                    { new Guid("bf56622f-c2b0-4228-8d12-04268f738630"), null, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2281), null, null, null, false, null, null, "SuperAdmin", "SUPERADMIN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Email", "EmailConfirmed", "ImageName", "IsDeleted", "IsFirstLogin", "IsLocked", "LastLoginDate", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifierId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("d6f236d9-179c-4993-bf7e-3cd3d6b1bf64"), 0, "22fbe6cc-20ae-40af-aa4e-9f43bc81961a", new DateTime(2025, 5, 28, 11, 21, 36, 527, DateTimeKind.Utc).AddTicks(7632), null, null, null, null, false, null, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, null, null, null, null, "SUPARADMIN", "AQAAAAIAAYagAAAAEFAxYEqOiTqX1Srsn3Ob545f0kBBphYaSRkHTPuE56GNf/NcT8j5izs6cMEzbDdNwA==", null, false, "a9a02fd6-2b99-4752-833f-f36351b8b54f", false, "superadmin" });

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 567, DateTimeKind.Local).AddTicks(762));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 567, DateTimeKind.Local).AddTicks(795));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 567, DateTimeKind.Local).AddTicks(797));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 567, DateTimeKind.Local).AddTicks(798));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 567, DateTimeKind.Local).AddTicks(798));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 567, DateTimeKind.Local).AddTicks(799));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 567, DateTimeKind.Local).AddTicks(800));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2520));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2522));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2523));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2524));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2525));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2526));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2527));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2629));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2630));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2632));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2633));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2634));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2635));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2636));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2638));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2640));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2641));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2643));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2644));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2645));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2646));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2648));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2649));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2649));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2650));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2651));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2652));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2655));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2656));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2656));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(6798));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(6804));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(6809));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(6811));
        }
    }
}
