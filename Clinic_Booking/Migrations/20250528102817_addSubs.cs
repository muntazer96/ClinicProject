using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class addSubs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<bool>(
                name: "EBooking",
                table: "SubscriptionPackages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EPayments",
                table: "SubscriptionPackages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MakeOffers",
                table: "SubscriptionPackages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxActiveOffers",
                table: "SubscriptionPackages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "SubscriptionPackages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ShowMessages",
                table: "SubscriptionPackages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "YearlyPrice",
                table: "SubscriptionPackages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Days",
                type: "text",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.InsertData(
                table: "Days",
                columns: new[] { "Id", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(9870), null, null, null, false, null, null, "السبت", "Saturday" },
                    { 2, new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(9872), null, null, null, false, null, null, "الاحد", "Sunday" },
                    { 3, new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(9874), null, null, null, false, null, null, "الاثنين", "Monday" },
                    { 4, new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(9874), null, null, null, false, null, null, "الثلاثاء", "Tuesday" },
                    { 5, new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(9875), null, null, null, false, null, null, "الاربعاء", "Wednesday" },
                    { 6, new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(9876), null, null, null, false, null, null, "الخميس", "Thursday" },
                    { 7, new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(9877), null, null, null, false, null, null, "الجمعة", "Friday" }
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPackages",
                columns: new[] { "Id", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "EBooking", "EPayments", "IsDeleted", "MakeOffers", "MaxActiveOffers", "MaxDailyAppointments", "MaxWeeklyDays", "ModifiedAt", "ModifierId", "Name", "NormalizedName", "Price", "ShowMessages", "ShowReviews", "YearlyPrice" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(5823), null, null, null, false, false, false, false, 0, 15, 4, null, null, "أساسي", "Basic", 0m, false, false, 0m },
                    { 2, new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(5831), null, null, null, false, false, false, false, 0, 25, 5, null, null, "ذهبي", "Gold", 25m, false, true, 250m },
                    { 3, new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(5836), null, null, null, true, true, false, true, 1, 35, 6, null, null, "ألماس", "Diamond ", 35m, true, true, 350m },
                    { 4, new DateTime(2025, 5, 28, 13, 28, 17, 28, DateTimeKind.Local).AddTicks(5838), null, null, null, true, true, false, true, 2, 1000, 7, null, null, "فاخر", "Premium", 45m, true, true, 450m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DeleteData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "EBooking",
                table: "SubscriptionPackages");

            migrationBuilder.DropColumn(
                name: "EPayments",
                table: "SubscriptionPackages");

            migrationBuilder.DropColumn(
                name: "MakeOffers",
                table: "SubscriptionPackages");

            migrationBuilder.DropColumn(
                name: "MaxActiveOffers",
                table: "SubscriptionPackages");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "SubscriptionPackages");

            migrationBuilder.DropColumn(
                name: "ShowMessages",
                table: "SubscriptionPackages");

            migrationBuilder.DropColumn(
                name: "YearlyPrice",
                table: "SubscriptionPackages");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Days");

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
    }
}
