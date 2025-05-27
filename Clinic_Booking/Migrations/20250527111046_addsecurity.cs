using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class addsecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bcdfc86a-fabb-4721-a2d2-ad1bf6834bbd"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("cec4a900-4ab1-4d00-b870-e699fc710efb"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("ead8609c-ccb0-419b-a45d-b22f5d93a855"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("78f80694-b939-42ae-be00-c55e94c6969c"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("02364cad-3511-42cd-8169-c98d2660427d"), null, new DateTime(2025, 5, 27, 14, 10, 46, 374, DateTimeKind.Local).AddTicks(9329), null, null, null, false, null, null, "NormalUser", "NORMALUSER" },
                    { new Guid("5f6a7d5e-d446-4bbc-80ca-fe54a37fcc67"), null, new DateTime(2025, 5, 27, 14, 10, 46, 374, DateTimeKind.Local).AddTicks(9316), null, null, null, false, null, null, "SuperAdmin", "SUPERADMIN" },
                    { new Guid("9b0b959e-fd53-4b4e-b9b9-db495af59cef"), null, new DateTime(2025, 5, 27, 14, 10, 46, 374, DateTimeKind.Local).AddTicks(9331), null, null, null, false, null, null, "DoctorUser", "DOCTORUSER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Email", "EmailConfirmed", "ImageName", "IsDeleted", "IsFirstLogin", "IsLocked", "LastLoginDate", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifierId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("37983523-f5d5-4577-8fc4-58f6d0f2da90"), 0, "b98d5ce4-aab1-4d99-a1ec-41d29e5f708f", new DateTime(2025, 5, 27, 11, 10, 46, 335, DateTimeKind.Utc).AddTicks(7210), null, null, null, null, false, null, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, null, null, null, null, "SUPARADMIN", "AQAAAAIAAYagAAAAEC1l9liPDmN/3T9/IHFgEVHyvrq2POIIN41guyGO454K2RO4xQUQvVmd38OYmTjfYA==", null, false, "718bcca7-6d9a-4a63-adc3-fe162ba9b18d", false, "superadmin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("02364cad-3511-42cd-8169-c98d2660427d"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("5f6a7d5e-d446-4bbc-80ca-fe54a37fcc67"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("9b0b959e-fd53-4b4e-b9b9-db495af59cef"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("37983523-f5d5-4577-8fc4-58f6d0f2da90"));

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("bcdfc86a-fabb-4721-a2d2-ad1bf6834bbd"), null, new DateTime(2025, 5, 26, 12, 1, 18, 829, DateTimeKind.Local).AddTicks(8884), null, null, null, false, null, null, "SuperAdmin", "SUPERADMIN" },
                    { new Guid("cec4a900-4ab1-4d00-b870-e699fc710efb"), null, new DateTime(2025, 5, 26, 12, 1, 18, 829, DateTimeKind.Local).AddTicks(8898), null, null, null, false, null, null, "DoctorUser", "DOCTORUSER" },
                    { new Guid("ead8609c-ccb0-419b-a45d-b22f5d93a855"), null, new DateTime(2025, 5, 26, 12, 1, 18, 829, DateTimeKind.Local).AddTicks(8897), null, null, null, false, null, null, "NormalUser", "NORMALUSER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Email", "EmailConfirmed", "ImageName", "IsDeleted", "IsFirstLogin", "IsLocked", "LastLoginDate", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifierId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("78f80694-b939-42ae-be00-c55e94c6969c"), 0, "cad8028b-16c3-4f53-951f-74ca32c9db92", new DateTime(2025, 5, 26, 9, 1, 18, 791, DateTimeKind.Utc).AddTicks(6824), null, null, null, null, false, null, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, null, null, null, null, "SUPARADMIN", "AQAAAAIAAYagAAAAEBmxkv50pXUIwi1JAua/UheMxPX03rEKsEdHYmQgKfzUncx9UlV/ns6IBAC+0WKrUg==", null, false, null, false, "superadmin" });
        }
    }
}
