using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class addDataToRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "ImageName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AspNetUsers");
        }
    }
}
