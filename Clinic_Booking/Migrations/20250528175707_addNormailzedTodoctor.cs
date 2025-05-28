using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class addNormailzedTodoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Doctors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("0cc820af-b995-4ee3-9b0c-48e8b15cc5d5"), null, new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(1602), null, null, null, false, null, null, "DoctorUser", "DOCTORUSER" },
                    { new Guid("aa815a1b-2b8d-48b7-b205-3076ec01289f"), null, new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(1563), null, null, null, false, null, null, "SuperAdmin", "SUPERADMIN" },
                    { new Guid("f40df783-795a-43ee-a3a2-55d6c302bfa0"), null, new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(1589), null, null, null, false, null, null, "NormalUser", "NORMALUSER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Email", "EmailConfirmed", "ImageName", "IsDeleted", "IsFirstLogin", "IsLocked", "LastLoginDate", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifierId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("7e3857c4-3cdb-40a9-82b8-53555df41d24"), 0, "1769020d-a6b2-45ff-960e-1131cf79bedc", new DateTime(2025, 5, 28, 17, 57, 6, 461, DateTimeKind.Utc).AddTicks(2743), null, null, null, null, false, null, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, null, null, null, null, "SUPARADMIN", "AQAAAAIAAYagAAAAEFY6LLbw+8/GtB5mYnxOlwOpADjH/9HpsC+F8rb28ojmXB6lWTxrFnZNYwb9PTTC/A==", null, false, "cc5b0b5b-dd92-412d-858f-093658dd0266", false, "superadmin" });

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 520, DateTimeKind.Local).AddTicks(3960));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 520, DateTimeKind.Local).AddTicks(3967));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 520, DateTimeKind.Local).AddTicks(3968));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 520, DateTimeKind.Local).AddTicks(3969));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 520, DateTimeKind.Local).AddTicks(3970));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 520, DateTimeKind.Local).AddTicks(4008));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 520, DateTimeKind.Local).AddTicks(4009));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2066));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2069));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2070));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2071));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2071));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2072));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2073));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2075));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2076));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2076));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2078));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2078));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2079));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2080));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2081));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2082));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2083));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2150));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2152));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2153));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2153));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2154));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2155));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2156));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2157));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2157));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2158));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2159));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2160));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 518, DateTimeKind.Local).AddTicks(2161));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 519, DateTimeKind.Local).AddTicks(1515));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 519, DateTimeKind.Local).AddTicks(1528));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 519, DateTimeKind.Local).AddTicks(1533));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 20, 57, 6, 519, DateTimeKind.Local).AddTicks(1535));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0cc820af-b995-4ee3-9b0c-48e8b15cc5d5"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("aa815a1b-2b8d-48b7-b205-3076ec01289f"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("f40df783-795a-43ee-a3a2-55d6c302bfa0"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("7e3857c4-3cdb-40a9-82b8-53555df41d24"));

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Doctors");

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
    }
}
