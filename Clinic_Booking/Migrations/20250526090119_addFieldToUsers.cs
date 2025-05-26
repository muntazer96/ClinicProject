using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class addFieldToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("46b60ee5-b2d9-457e-845c-6eb032217d2b"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("83f61553-5ad6-4242-9c16-2a5d281a79ca"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("b5a131d7-9b5a-41bd-9c78-8eb92e5856c9"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("ed02984f-0dfd-4520-8a47-1a8f4bbf76e1"));

            migrationBuilder.AddColumn<bool>(
                name: "IsFirstLogin",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginDate",
                table: "AspNetUsers",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "IsFirstLogin",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastLoginDate",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("46b60ee5-b2d9-457e-845c-6eb032217d2b"), null, new DateTime(2025, 5, 26, 11, 21, 52, 184, DateTimeKind.Local).AddTicks(8716), null, null, null, false, null, null, "DoctorUser", "DOCTORUSER" },
                    { new Guid("83f61553-5ad6-4242-9c16-2a5d281a79ca"), null, new DateTime(2025, 5, 26, 11, 21, 52, 184, DateTimeKind.Local).AddTicks(8629), null, null, null, false, null, null, "SuperAdmin", "SUPERADMIN" },
                    { new Guid("b5a131d7-9b5a-41bd-9c78-8eb92e5856c9"), null, new DateTime(2025, 5, 26, 11, 21, 52, 184, DateTimeKind.Local).AddTicks(8713), null, null, null, false, null, null, "NormalUser", "NORMALUSER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Email", "EmailConfirmed", "ImageName", "IsDeleted", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifierId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("ed02984f-0dfd-4520-8a47-1a8f4bbf76e1"), 0, "0d45bd5c-9dd1-4f3c-a8d6-b3bc7b030dd2", new DateTime(2025, 5, 26, 8, 21, 52, 144, DateTimeKind.Utc).AddTicks(2746), null, null, null, null, false, null, false, false, null, null, null, null, null, "SUPARADMIN", "AQAAAAIAAYagAAAAEMvJ0KGHDGWxET+Fx9/gZRmrEgfEVroqtP8TEhnZMRqS1lAYp+mgW33fcSX4uQu8+Q==", null, false, null, false, "superadmin" });
        }
    }
}
