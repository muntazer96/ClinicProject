using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class addNewSpe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("db3946f4-275b-4bc1-ac3a-a6e1f3f4badb"),
                column: "ConcurrencyStamp",
                value: "1b1456ed-8219-4aac-974a-6676c54ed0f3");

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 617, DateTimeKind.Unspecified).AddTicks(5001));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 617, DateTimeKind.Unspecified).AddTicks(5013));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 617, DateTimeKind.Unspecified).AddTicks(5015));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 617, DateTimeKind.Unspecified).AddTicks(5017));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 617, DateTimeKind.Unspecified).AddTicks(5018));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 617, DateTimeKind.Unspecified).AddTicks(5019));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 617, DateTimeKind.Unspecified).AddTicks(5021));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 617, DateTimeKind.Unspecified).AddTicks(7396));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 617, DateTimeKind.Unspecified).AddTicks(7407));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 617, DateTimeKind.Unspecified).AddTicks(7409));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 617, DateTimeKind.Unspecified).AddTicks(7411));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3115));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3123));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3124));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3126));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3127));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3129));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3130));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3131));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3133));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3134));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3135));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3136));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3138));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3139));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3140));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3142));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3143));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3144));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3146));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3147));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3148));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3150));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3151));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3152));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3153));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3155));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3156));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3191));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3193));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3195));

            migrationBuilder.InsertData(
                table: "Specializations",
                columns: new[] { "Id", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IconName", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { 31, new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3196), null, null, null, "urology", false, null, null, "أخصائي مسالك بولية", "Urologist" },
                    { 32, new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3197), null, null, null, "vascular-surgery", false, null, null, "أخصائي جراحة الأوعية الدموية", "Vascular Surgeon" },
                    { 33, new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3199), null, null, null, "thoracic-surgery", false, null, null, "أخصائي جراحة الصدر", "Thoracic Surgeon" },
                    { 34, new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3200), null, null, null, "cardiac-surgery", false, null, null, "أخصائي جراحة القلب", "Cardiac Surgeon" },
                    { 35, new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3201), null, null, null, "dentistry", false, null, null, "أخصائي طب الأسنان", "Dentist" },
                    { 36, new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3203), null, null, null, "orthodontics", false, null, null, "أخصائي تقويم الأسنان", "Orthodontist" },
                    { 37, new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3204), null, null, null, "oral-surgery", false, null, null, "أخصائي جراحة الفم والفكين", "Oral and Maxillofacial Surgeon" },
                    { 38, new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3205), null, null, null, "allergy-immunology", false, null, null, "أخصائي أمراض الحساسية والمناعة", "Allergist and Immunologist" },
                    { 39, new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3207), null, null, null, "nuclear-medicine", false, null, null, "أخصائي الطب النووي", "Nuclear Medicine Specialist" },
                    { 40, new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3208), null, null, null, "intensive-care", false, null, null, "أخصائي العناية المركزة", "Intensive Care Specialist" },
                    { 41, new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3209), null, null, null, "rehabilitation", false, null, null, "أخصائي إعادة التأهيل", "Physical Medicine and Rehabilitation Specialist" },
                    { 42, new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3211), null, null, null, "geriatrics", false, null, null, "أخصائي طب الشيخوخة", "Geriatrician" },
                    { 43, new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3212), null, null, null, "forensic-medicine", false, null, null, "أخصائي الطب الشرعي", "Forensic Medicine Specialist" },
                    { 44, new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3213), null, null, null, "medical-genetics", false, null, null, "أخصائي الوراثة الطبية", "Medical Geneticist" }
                });

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 614, DateTimeKind.Unspecified).AddTicks(5456));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 614, DateTimeKind.Unspecified).AddTicks(5472));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 614, DateTimeKind.Unspecified).AddTicks(5477));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 614, DateTimeKind.Unspecified).AddTicks(5479));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("db3946f4-275b-4bc1-ac3a-a6e1f3f4badb"),
                column: "ConcurrencyStamp",
                value: "ee174454-2175-44a7-9133-3a3280c7af38");

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 841, DateTimeKind.Unspecified).AddTicks(1501));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 841, DateTimeKind.Unspecified).AddTicks(1518));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 841, DateTimeKind.Unspecified).AddTicks(1519));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 841, DateTimeKind.Unspecified).AddTicks(1521));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 841, DateTimeKind.Unspecified).AddTicks(1522));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 841, DateTimeKind.Unspecified).AddTicks(1523));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 841, DateTimeKind.Unspecified).AddTicks(1547));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 841, DateTimeKind.Unspecified).AddTicks(2869));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 841, DateTimeKind.Unspecified).AddTicks(2880));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 841, DateTimeKind.Unspecified).AddTicks(2881));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 841, DateTimeKind.Unspecified).AddTicks(2883));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3921));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3924));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3925));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3927));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3928));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3929));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3930));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3931));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3932));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3933));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3934));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3935));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3936));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3937));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3939));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3940));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3941));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3942));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3943));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3944));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3945));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3946));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3947));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3948));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3949));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3950));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3952));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3974));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3976));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(3977));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(9827));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(9846));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(9851));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 12, 1, 43, 839, DateTimeKind.Unspecified).AddTicks(9854));
        }
    }
}
