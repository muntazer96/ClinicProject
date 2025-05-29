using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class addLocationPhoneForDoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("1a3b4179-ab43-4c93-bf6a-0fdf3ffb4af0"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("542c39eb-9ffa-4eca-b150-0bf608f2450b"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("70d68378-8538-4e87-88ba-7384bf94f1b1"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("f552549e-59a5-4c10-ae9f-69876286e997"));

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Doctors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Doctors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("2172d76b-8d30-4b0b-8396-afbe07672721"), null, new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(755), null, null, null, false, null, null, "SuperAdmin", "SUPERADMIN" },
                    { new Guid("2669efff-5c59-44ed-b9b2-07b4c23d4913"), null, new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(795), null, null, null, false, null, null, "NormalUser", "NORMALUSER" },
                    { new Guid("36919c19-e719-42b1-b494-82fddd725589"), null, new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(797), null, null, null, false, null, null, "DoctorUser", "DOCTORUSER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Email", "EmailConfirmed", "ImageName", "IsDeleted", "IsFirstLogin", "IsLocked", "LastLoginDate", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifierId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("c5681c7f-0d42-4d2c-9732-13d56ff7d4fc"), 0, "c73669a7-9bb6-4618-a98e-db3bb0bc669e", new DateTime(2025, 5, 29, 16, 21, 3, 115, DateTimeKind.Utc).AddTicks(1106), null, null, null, null, false, null, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, null, null, null, null, "SUPARADMIN", "AQAAAAIAAYagAAAAEKvbp8Mje0QoTed1TnRGPe0F/h25iUvU0GYYKPbGYbkyLmP/IiamQcNMSWIInnPIkg==", null, false, "cdb0785e-0f85-4a21-b248-56b78dfcbfe0", false, "superadmin" });

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 177, DateTimeKind.Local).AddTicks(801));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 177, DateTimeKind.Local).AddTicks(807));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 177, DateTimeKind.Local).AddTicks(808));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 177, DateTimeKind.Local).AddTicks(809));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 177, DateTimeKind.Local).AddTicks(810));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 177, DateTimeKind.Local).AddTicks(811));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 177, DateTimeKind.Local).AddTicks(811));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 177, DateTimeKind.Local).AddTicks(2493));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 177, DateTimeKind.Local).AddTicks(2499));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 177, DateTimeKind.Local).AddTicks(2500));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 177, DateTimeKind.Local).AddTicks(2501));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1227));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1230));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1231));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1232));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1233));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1234));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1235));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1235));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1236));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1237));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1237));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1238));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1239));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1240));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1241));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1241));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1242));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1243));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1244));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1244));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1245));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1246));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1247));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1247));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1248));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1249));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1250));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1250));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1251));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(1252));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(9614));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(9624));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(9631));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 19, 21, 3, 175, DateTimeKind.Local).AddTicks(9633));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("2172d76b-8d30-4b0b-8396-afbe07672721"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("2669efff-5c59-44ed-b9b2-07b4c23d4913"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("36919c19-e719-42b1-b494-82fddd725589"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("c5681c7f-0d42-4d2c-9732-13d56ff7d4fc"));

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Doctors");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("1a3b4179-ab43-4c93-bf6a-0fdf3ffb4af0"), null, new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(8861), null, null, null, false, null, null, "DoctorUser", "DOCTORUSER" },
                    { new Guid("542c39eb-9ffa-4eca-b150-0bf608f2450b"), null, new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(8859), null, null, null, false, null, null, "NormalUser", "NORMALUSER" },
                    { new Guid("70d68378-8538-4e87-88ba-7384bf94f1b1"), null, new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(8831), null, null, null, false, null, null, "SuperAdmin", "SUPERADMIN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Email", "EmailConfirmed", "ImageName", "IsDeleted", "IsFirstLogin", "IsLocked", "LastLoginDate", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifierId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("f552549e-59a5-4c10-ae9f-69876286e997"), 0, "b5fca736-a75a-47ad-97c1-0b62fab03a9c", new DateTime(2025, 5, 28, 22, 7, 4, 592, DateTimeKind.Utc).AddTicks(3204), null, null, null, null, false, null, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, null, null, null, null, "SUPARADMIN", "AQAAAAIAAYagAAAAEOKc1LfJqixutCcM6k22fNpl24PX/ZJJkzDZl5TCBqHNKBMonGYC6pPxUJRkmdOf2A==", null, false, "7dbaa920-4478-4db1-9b78-dea04ac44a4b", false, "superadmin" });

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 655, DateTimeKind.Local).AddTicks(6822));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 655, DateTimeKind.Local).AddTicks(6832));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 655, DateTimeKind.Local).AddTicks(6833));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 655, DateTimeKind.Local).AddTicks(6834));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 655, DateTimeKind.Local).AddTicks(6835));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 655, DateTimeKind.Local).AddTicks(6836));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 655, DateTimeKind.Local).AddTicks(6836));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 655, DateTimeKind.Local).AddTicks(9507));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 655, DateTimeKind.Local).AddTicks(9515));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 655, DateTimeKind.Local).AddTicks(9516));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 655, DateTimeKind.Local).AddTicks(9517));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9323));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9327));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9328));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9329));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9329));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9330));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9331));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9332));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9333));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9334));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9334));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9335));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9336));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9425));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9426));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9428));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9428));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9429));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9430));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9431));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9432));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9433));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9433));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9434));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9435));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9436));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9446));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9460));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9461));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 652, DateTimeKind.Local).AddTicks(9462));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 654, DateTimeKind.Local).AddTicks(654));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 654, DateTimeKind.Local).AddTicks(668));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 654, DateTimeKind.Local).AddTicks(672));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 29, 1, 7, 4, 654, DateTimeKind.Local).AddTicks(675));
        }
    }
}
