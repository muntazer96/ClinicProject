using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class addSpec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("85f78d54-e0c3-444d-9aec-817de1eadbcb"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("97eff174-32b1-4486-9c1c-19d687955c77"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("be5f01e0-a34e-41e9-98b5-0b34ddee48c3"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("70c80de5-3829-4242-b691-db4f6dbb4576"));

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Specializations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("00a4590e-163d-4316-9120-2876b18962f2"), null, new DateTime(2025, 5, 28, 14, 19, 45, 603, DateTimeKind.Local).AddTicks(8234), null, null, null, false, null, null, "DoctorUser", "DOCTORUSER" },
                    { new Guid("b1fdc550-536f-465c-972f-28967e1d4fec"), null, new DateTime(2025, 5, 28, 14, 19, 45, 603, DateTimeKind.Local).AddTicks(8233), null, null, null, false, null, null, "NormalUser", "NORMALUSER" },
                    { new Guid("dd972687-810f-422a-ae8c-1a546e4c44a7"), null, new DateTime(2025, 5, 28, 14, 19, 45, 603, DateTimeKind.Local).AddTicks(8214), null, null, null, false, null, null, "SuperAdmin", "SUPERADMIN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Email", "EmailConfirmed", "ImageName", "IsDeleted", "IsFirstLogin", "IsLocked", "LastLoginDate", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifierId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("889731ab-6c0c-4d0e-99c3-cb8de41afb6a"), 0, "bf53b60d-9d55-48f6-8b21-7993705e4ebb", new DateTime(2025, 5, 28, 11, 19, 45, 565, DateTimeKind.Utc).AddTicks(4673), null, null, null, null, false, null, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, null, null, null, null, "SUPARADMIN", "AQAAAAIAAYagAAAAEKh8CSRjWx0ZDz1RmaeNrsndY756+ocOwbqDczHtzyMuTwL98L/FAtaFWg6LAJ/7tA==", null, false, "caa3f431-41de-4e0f-aabb-c8f070a54300", false, "superadmin" });

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 19, 45, 604, DateTimeKind.Local).AddTicks(5486));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 19, 45, 604, DateTimeKind.Local).AddTicks(5489));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 19, 45, 604, DateTimeKind.Local).AddTicks(5490));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 19, 45, 604, DateTimeKind.Local).AddTicks(5491));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 19, 45, 604, DateTimeKind.Local).AddTicks(5491));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 19, 45, 604, DateTimeKind.Local).AddTicks(5492));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 19, 45, 604, DateTimeKind.Local).AddTicks(5493));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 19, 45, 604, DateTimeKind.Local).AddTicks(2066));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 19, 45, 604, DateTimeKind.Local).AddTicks(2076));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 19, 45, 604, DateTimeKind.Local).AddTicks(2081));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 19, 45, 604, DateTimeKind.Local).AddTicks(2083));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("00a4590e-163d-4316-9120-2876b18962f2"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("b1fdc550-536f-465c-972f-28967e1d4fec"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("dd972687-810f-422a-ae8c-1a546e4c44a7"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("889731ab-6c0c-4d0e-99c3-cb8de41afb6a"));

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Specializations");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("85f78d54-e0c3-444d-9aec-817de1eadbcb"), null, new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(1293), null, null, null, false, null, null, "SuperAdmin", "SUPERADMIN" },
                    { new Guid("97eff174-32b1-4486-9c1c-19d687955c77"), null, new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(1314), null, null, null, false, null, null, "NormalUser", "NORMALUSER" },
                    { new Guid("be5f01e0-a34e-41e9-98b5-0b34ddee48c3"), null, new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(1315), null, null, null, false, null, null, "DoctorUser", "DOCTORUSER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Email", "EmailConfirmed", "ImageName", "IsDeleted", "IsFirstLogin", "IsLocked", "LastLoginDate", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifierId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("70c80de5-3829-4242-b691-db4f6dbb4576"), 0, "5e59106a-b229-437c-8a8e-1763d8a4df64", new DateTime(2025, 5, 28, 10, 28, 16, 988, DateTimeKind.Utc).AddTicks(2415), null, null, null, null, false, null, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, null, null, null, null, "SUPARADMIN", "AQAAAAIAAYagAAAAEB5rY+h5rG7avqb70r7HiaitxEJJIcar7O6enqQqu8JjSSJ2ru334h3zIprCHnmuFA==", null, false, "a368dea1-5791-4b73-b5c7-da1393a270da", false, "superadmin" });

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(9870));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(9872));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(9874));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(9874));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(9875));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(9876));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(9877));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(5823));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(5831));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(5836));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(5838));
        }
    }
}
