using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class addFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Features",
                type: "text",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.InsertData(
                table: "Features",
                columns: new[] { "Id", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Description", "IsDeleted", "IsPremiumOnly", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 5, 29, 1, 7, 4, 655, DateTimeKind.Local).AddTicks(9507), null, null, null, "", false, true, null, null, "تفعيل التقييم والردود", "ShowReviews" },
                    { 2, new DateTime(2025, 5, 29, 1, 7, 4, 655, DateTimeKind.Local).AddTicks(9515), null, null, null, "", false, true, null, null, "تفعيل زر الرسائل", "ShowMessages" },
                    { 3, new DateTime(2025, 5, 29, 1, 7, 4, 655, DateTimeKind.Local).AddTicks(9516), null, null, null, "", false, true, null, null, "تفعيل الحجز الالكتروني", "EBooking" },
                    { 4, new DateTime(2025, 5, 29, 1, 7, 4, 655, DateTimeKind.Local).AddTicks(9517), null, null, null, "", false, true, null, null, "تفعيل الدفع الالكتروني", "EPayments" }
                });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Features");

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
    }
}
