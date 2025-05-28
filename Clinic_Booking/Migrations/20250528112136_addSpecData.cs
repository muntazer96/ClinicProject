using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class addSpecData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("28dd0b27-8929-4d5c-956d-70537bd756d3"), null, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2294), null, null, null, false, null, null, "NormalUser", "NORMALUSER" },
                    { new Guid("bb3f3792-b4e8-460f-a977-3d95029cbb6c"), null, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2296), null, null, null, false, null, null, "DoctorUser", "DOCTORUSER" },
                    { new Guid("bf56622f-c2b0-4228-8d12-04268f738630"), null, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2281), null, null, null, false, null, null, "SuperAdmin", "SUPERADMIN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Email", "EmailConfirmed", "ImageName", "IsDeleted", "IsFirstLogin", "IsLocked", "LastLoginDate", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifierId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("d6f236d9-179c-4993-bf7e-3cd3d6b1bf64"), 0, "22fbe6cc-20ae-40af-aa4e-9f43bc81961a", new DateTime(2025, 5, 28, 11, 21, 36, 527, DateTimeKind.Utc).AddTicks(7632), null, null, null, null, false, null, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, null, null, null, null, "SUPARADMIN", "AQAAAAIAAYagAAAAEFAxYEqOiTqX1Srsn3Ob545f0kBBphYaSRkHTPuE56GNf/NcT8j5izs6cMEzbDdNwA==", null, false, "a9a02fd6-2b99-4752-833f-f36351b8b54f", false, "superadmin" });

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 567, DateTimeKind.Local).AddTicks(762));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 567, DateTimeKind.Local).AddTicks(795));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 567, DateTimeKind.Local).AddTicks(797));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 567, DateTimeKind.Local).AddTicks(798));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 567, DateTimeKind.Local).AddTicks(798));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 567, DateTimeKind.Local).AddTicks(799));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 567, DateTimeKind.Local).AddTicks(800));

            migrationBuilder.InsertData(
                table: "Specializations",
                columns: new[] { "Id", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2520), null, null, null, false, null, null, "أخصائي باطنية", "Internal Medicine Specialist" },
                    { 2, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2522), null, null, null, false, null, null, "أخصائي أنف وأذن وحنجرة", "ENT Specialist (Ear, Nose, and Throat)" },
                    { 3, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2523), null, null, null, false, null, null, "أخصائي قلب", "Cardiologist" },
                    { 4, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2524), null, null, null, false, null, null, "أخصائي عيون", "Ophthalmologist" },
                    { 5, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2525), null, null, null, false, null, null, "أخصائي جلدية", "Dermatologist" },
                    { 6, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2526), null, null, null, false, null, null, "أخصائي أعصاب", "Neurologist" },
                    { 7, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2527), null, null, null, false, null, null, "أخصائي جراحة عامة", "General Surgeon" },
                    { 8, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2629), null, null, null, false, null, null, "أخصائي جراحة عظام", "Orthopedic Surgeon" },
                    { 9, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2630), null, null, null, false, null, null, "أخصائي نسائية وتوليد", "Obstetrician-Gynecologist (OB-GYN)" },
                    { 10, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2632), null, null, null, false, null, null, "أخصائي أطفال", "Pediatrician" },
                    { 11, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2633), null, null, null, false, null, null, "أخصائي أورام", "Oncologist" },
                    { 12, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2634), null, null, null, false, null, null, "أخصائي كلى", "Nephrologist" },
                    { 13, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2635), null, null, null, false, null, null, "أخصائي جهاز هضمي", "Gastroenterologist" },
                    { 14, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2636), null, null, null, false, null, null, "أخصائي غدد صماء", "Endocrinologist" },
                    { 15, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2638), null, null, null, false, null, null, "أخصائي جراحة تجميل", "Plastic Surgeon" },
                    { 16, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2640), null, null, null, false, null, null, "أخصائي جراحة دماغ وأعصاب", "Neurosurgeon" },
                    { 17, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2641), null, null, null, false, null, null, "أخصائي تخدير", "Anesthesiologist" },
                    { 18, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2643), null, null, null, false, null, null, "أخصائي طب الأسرة", "Family Medicine Specialist" },
                    { 19, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2644), null, null, null, false, null, null, "أخصائي الطب النفسي", "Psychiatrist" },
                    { 20, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2645), null, null, null, false, null, null, "أخصائي أمراض معدية", "Infectious Disease Specialist" },
                    { 21, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2646), null, null, null, false, null, null, "أخصائي أشعة", "Radiologist" },
                    { 22, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2648), null, null, null, false, null, null, "أخصائي طب طوارئ", "Emergency Medicine Specialist" },
                    { 23, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2649), null, null, null, false, null, null, "أخصائي روماتيزم", "Rheumatologist" },
                    { 24, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2649), null, null, null, false, null, null, "أخصائي صدرية", "Pulmonologist" },
                    { 25, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2650), null, null, null, false, null, null, "أخصائي طب مهني", "Occupational Medicine Specialist" },
                    { 26, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2651), null, null, null, false, null, null, "أخصائي طب رياضي", "Sports Medicine Specialist" },
                    { 27, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2652), null, null, null, false, null, null, "أخصائي أمراض الدم", "Hematologist" },
                    { 28, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2655), null, null, null, false, null, null, "أخصائي علاج طبيعي", "Physiotherapist" },
                    { 29, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2656), null, null, null, false, null, null, "أخصائي تغذية", "Nutritionist" },
                    { 30, new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(2656), null, null, null, false, null, null, "أخصائي نطق وتخاطب", "Speech Therapist" }
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(6798));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(6804));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(6809));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 5, 28, 14, 21, 36, 566, DateTimeKind.Local).AddTicks(6811));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("28dd0b27-8929-4d5c-956d-70537bd756d3"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bb3f3792-b4e8-460f-a977-3d95029cbb6c"));

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("bf56622f-c2b0-4228-8d12-04268f738630"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("d6f236d9-179c-4993-bf7e-3cd3d6b1bf64"));

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30);

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
    }
}
