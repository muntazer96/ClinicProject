using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class addLinkBetweenReviewAndApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Reviews",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<int>(
                name: "AppoinmentId",
                table: "Reviews",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Doctors",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Doctors",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("58f4d9bb-3427-4e38-8e72-16306d1d5e3e"), null, new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1409), null, null, null, false, null, null, "DoctorUser", "DOCTORUSER" },
                    { new Guid("6debed21-a066-4aa3-85b7-0b5a89cf7599"), null, new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1407), null, null, null, false, null, null, "NormalUser", "NORMALUSER" },
                    { new Guid("f957d910-9a31-4ba8-b93a-99688d01f375"), null, new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1392), null, null, null, false, null, null, "SuperAdmin", "SUPERADMIN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Email", "EmailConfirmed", "ImageName", "IsDeleted", "IsFirstLogin", "IsLocked", "LastLoginDate", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifierId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("1a08b6a6-fecf-4a10-a778-e16814594d60"), 0, "ff066069-b6c2-47b6-9a3b-92ee2155e30b", new DateTime(2025, 6, 4, 17, 53, 20, 888, DateTimeKind.Utc).AddTicks(2402), null, null, null, null, false, null, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, null, null, null, null, "SUPARADMIN", "AQAAAAIAAYagAAAAEF+WyN21LRFE444xXEped96VXGFgrK+oI60ocLGA8I0YABhjRPZ2MkRSQK4iSXwAxw==", null, false, "abd960fe-e4d5-4901-91df-587cd07f946f", false, "superadmin" });

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 952, DateTimeKind.Local).AddTicks(8570));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 952, DateTimeKind.Local).AddTicks(8578));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 952, DateTimeKind.Local).AddTicks(8579));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 952, DateTimeKind.Local).AddTicks(8580));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 952, DateTimeKind.Local).AddTicks(8581));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 952, DateTimeKind.Local).AddTicks(8581));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 952, DateTimeKind.Local).AddTicks(8582));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 953, DateTimeKind.Local).AddTicks(1579));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 953, DateTimeKind.Local).AddTicks(1586));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 953, DateTimeKind.Local).AddTicks(1588));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 953, DateTimeKind.Local).AddTicks(1589));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1758));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1761));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1762));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1763));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1764));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1765));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1766));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1766));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1767));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1768));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1769));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1770));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1770));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1771));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1772));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1773));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1774));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1774));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1775));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1776));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1777));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1778));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1778));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1779));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1780));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1781));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1782));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1783));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1783));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 944, DateTimeKind.Local).AddTicks(1784));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 945, DateTimeKind.Local).AddTicks(992));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 945, DateTimeKind.Local).AddTicks(1000));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 945, DateTimeKind.Local).AddTicks(1005));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 4, 20, 53, 20, 945, DateTimeKind.Local).AddTicks(1008));

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_AppoinmentId",
                table: "Reviews",
                column: "AppoinmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Appointments_AppoinmentId",
                table: "Reviews",
                column: "AppoinmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Appointments_AppoinmentId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_AppoinmentId",
                table: "Reviews");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("58f4d9bb-3427-4e38-8e72-16306d1d5e3e"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("6debed21-a066-4aa3-85b7-0b5a89cf7599"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("f957d910-9a31-4ba8-b93a-99688d01f375"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("1a08b6a6-fecf-4a10-a778-e16814594d60"));

            migrationBuilder.DropColumn(
                name: "AppoinmentId",
                table: "Reviews");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Reviews",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Doctors",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Doctors",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

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
    }
}
