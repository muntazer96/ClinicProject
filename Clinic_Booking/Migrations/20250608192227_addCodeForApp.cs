using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class addCodeForApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("090b126a-57e0-470e-9c0a-5daef229487a"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0cf13926-eace-45be-8ee5-7d32b1ffc7d2"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("60e0b2ce-1432-4e9d-a261-2d534bf0ebaf"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("b9d7b525-b00f-447e-b50d-39fc00a5406b"));

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Appointments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("00a49b8c-dfef-4d6a-aa52-9d872936b523"), null, new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3160), null, null, null, false, null, null, "SuperAdmin", "SUPERADMIN" },
                    { new Guid("8babf235-ca0b-4e0f-a645-f8e97d659598"), null, new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3186), null, null, null, false, null, null, "DoctorUser", "DOCTORUSER" },
                    { new Guid("fb2757cc-fe02-478a-9a49-7103566879eb"), null, new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3184), null, null, null, false, null, null, "NormalUser", "NORMALUSER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Email", "EmailConfirmed", "ImageName", "IsDeleted", "IsFirstLogin", "IsLocked", "LastLoginDate", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifierId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("65afe713-0f8f-4822-aed3-5df510502f1a"), 0, "3fbabba7-7ae9-40e5-8e6f-22f47341ad7f", new DateTime(2025, 6, 8, 19, 22, 25, 540, DateTimeKind.Utc).AddTicks(9375), null, null, null, null, false, null, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, null, null, null, null, "SUPARADMIN", "AQAAAAIAAYagAAAAEJNUOOV63KkXWy3NNLrwpS2acmSKFc5MCd7SPxSCVQIwFdwqk6jvPF5PDAOCPoW26g==", null, false, "4d5423af-e08c-426a-99f6-0e20fe23eb86", false, "superadmin" });

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 605, DateTimeKind.Local).AddTicks(5856));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 605, DateTimeKind.Local).AddTicks(5865));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 605, DateTimeKind.Local).AddTicks(5866));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 605, DateTimeKind.Local).AddTicks(5867));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 605, DateTimeKind.Local).AddTicks(5868));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 605, DateTimeKind.Local).AddTicks(5869));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 605, DateTimeKind.Local).AddTicks(5870));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 605, DateTimeKind.Local).AddTicks(8282));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 605, DateTimeKind.Local).AddTicks(8289));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 605, DateTimeKind.Local).AddTicks(8291));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 605, DateTimeKind.Local).AddTicks(8292));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3764));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3767));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3768));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3770));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3771));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3771));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3772));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3773));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3774));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3775));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3776));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3777));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3778));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3779));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3780));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3781));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3782));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3782));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3783));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3784));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3785));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3786));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3787));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3788));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3789));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3790));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3791));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3791));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3792));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 602, DateTimeKind.Local).AddTicks(3793));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 603, DateTimeKind.Local).AddTicks(5324));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 603, DateTimeKind.Local).AddTicks(5334));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 603, DateTimeKind.Local).AddTicks(5339));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 22, 22, 25, 603, DateTimeKind.Local).AddTicks(5341));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("00a49b8c-dfef-4d6a-aa52-9d872936b523"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("8babf235-ca0b-4e0f-a645-f8e97d659598"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("fb2757cc-fe02-478a-9a49-7103566879eb"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("65afe713-0f8f-4822-aed3-5df510502f1a"));

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Appointments");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("090b126a-57e0-470e-9c0a-5daef229487a"), null, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8581), null, null, null, false, null, null, "SuperAdmin", "SUPERADMIN" },
                    { new Guid("0cf13926-eace-45be-8ee5-7d32b1ffc7d2"), null, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8594), null, null, null, false, null, null, "DoctorUser", "DOCTORUSER" },
                    { new Guid("60e0b2ce-1432-4e9d-a261-2d534bf0ebaf"), null, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8592), null, null, null, false, null, null, "NormalUser", "NORMALUSER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Email", "EmailConfirmed", "ImageName", "IsDeleted", "IsFirstLogin", "IsLocked", "LastLoginDate", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifierId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("b9d7b525-b00f-447e-b50d-39fc00a5406b"), 0, "0d506b68-b10c-4490-be01-635ebe18aa0a", new DateTime(2025, 6, 8, 18, 47, 11, 249, DateTimeKind.Utc).AddTicks(3685), null, null, null, null, false, null, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, null, null, null, null, "SUPARADMIN", "AQAAAAIAAYagAAAAELjaF/WDs9p8Hj6Xh7hwUIde3loNch3BldIkdBRW1GADNbzqQ41dxSBiokPTYfnHzg==", null, false, "0ae899e5-c533-40ae-98dd-5378fd8917d8", false, "superadmin" });

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(3807));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(3812));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(3813));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(3814));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(3814));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(3815));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(3816));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(5801));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(5807));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(5808));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(5809));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8952));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8955));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8956));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8957));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8958));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8959));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8959));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8960));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8961));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8962));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8963));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8963));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8964));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8965));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8965));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8966));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8967));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8968));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8968));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8969));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8970));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8970));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8971));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8972));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8972));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8973));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8974));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8975));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8976));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8977));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 304, DateTimeKind.Local).AddTicks(7547));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 304, DateTimeKind.Local).AddTicks(7555));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 304, DateTimeKind.Local).AddTicks(7560));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 8, 21, 47, 11, 304, DateTimeKind.Local).AddTicks(7562));
        }
    }
}
