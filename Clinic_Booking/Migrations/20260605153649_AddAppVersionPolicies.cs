using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class AddAppVersionPolicies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppVersionPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Platform = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    LatestVersion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    LatestBuildNumber = table.Column<int>(type: "integer", nullable: false),
                    MinimumSupportedVersion = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MinimumSupportedBuildNumber = table.Column<int>(type: "integer", nullable: false),
                    ForceUpdate = table.Column<bool>(type: "boolean", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Message = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    UpdateUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppVersionPolicies", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("db3946f4-275b-4bc1-ac3a-a6e1f3f4badb"),
                column: "ConcurrencyStamp",
                value: "e3ee3cf1-4ac1-49e3-ac40-3cb088952270");

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 991, DateTimeKind.Local).AddTicks(2568));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 991, DateTimeKind.Local).AddTicks(2586));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 991, DateTimeKind.Local).AddTicks(2587));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 991, DateTimeKind.Local).AddTicks(2588));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 991, DateTimeKind.Local).AddTicks(2589));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 991, DateTimeKind.Local).AddTicks(2591));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 991, DateTimeKind.Local).AddTicks(2592));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 991, DateTimeKind.Local).AddTicks(5212));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 991, DateTimeKind.Local).AddTicks(5219));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 991, DateTimeKind.Local).AddTicks(5220));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 991, DateTimeKind.Local).AddTicks(5222));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(349));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(352));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(354));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(354));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(355));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(356));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(357));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(358));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(359));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(360));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(361));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(361));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(362));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(363));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(365));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(366));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(367));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(368));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(368));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(369));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(370));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(371));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(371));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(372));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(373));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(374));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(374));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(375));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(376));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 986, DateTimeKind.Local).AddTicks(377));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 987, DateTimeKind.Local).AddTicks(4633));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 987, DateTimeKind.Local).AddTicks(4643));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 987, DateTimeKind.Local).AddTicks(4648));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 18, 36, 48, 987, DateTimeKind.Local).AddTicks(4650));

            migrationBuilder.CreateIndex(
                name: "IX_AppVersionPolicies_Platform",
                table: "AppVersionPolicies",
                column: "Platform",
                unique: true);

            migrationBuilder.InsertData(
                table: "AppVersionPolicies",
                columns: new[]
                {
                    "Id",
                    "Platform",
                    "LatestVersion",
                    "LatestBuildNumber",
                    "MinimumSupportedVersion",
                    "MinimumSupportedBuildNumber",
                    "ForceUpdate",
                    "IsEnabled",
                    "Title",
                    "Message",
                    "UpdateUrl",
                    "IsDeleted",
                    "CreatedAt"
                },
                values: new object[,]
                {
                    { 1, "android", "0.0.0", 0, "0.0.0", 0, false, true, "تحديث جديد متوفر", "تتوفر نسخة أحدث من التطبيق. يرجى التحديث للحصول على أفضل تجربة.", null, false, new DateTime(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "ios", "0.0.0", 0, "0.0.0", 0, false, true, "تحديث جديد متوفر", "تتوفر نسخة أحدث من التطبيق. يرجى التحديث للحصول على أفضل تجربة.", null, false, new DateTime(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "web", "0.0.0", 0, "0.0.0", 0, false, true, "تحديث جديد متوفر", "تتوفر نسخة أحدث من التطبيق. يرجى التحديث للحصول على أفضل تجربة.", null, false, new DateTime(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "admin", "0.0.0", 0, "0.0.0", 0, false, true, "تحديث لوحة التحكم", "تتوفر نسخة أحدث من لوحة التحكم.", null, false, new DateTime(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "backend", "0.0.0", 0, "0.0.0", 0, false, true, "تحديث الخادم", "تم تسجيل نسخة أحدث من الخادم.", null, false, new DateTime(2026, 6, 5, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppVersionPolicies");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("db3946f4-275b-4bc1-ac3a-a6e1f3f4badb"),
                column: "ConcurrencyStamp",
                value: "5ff92d7b-9b9e-4fe3-ab07-6d5287b9d38f");

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 389, DateTimeKind.Local).AddTicks(9208));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 389, DateTimeKind.Local).AddTicks(9217));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 389, DateTimeKind.Local).AddTicks(9219));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 389, DateTimeKind.Local).AddTicks(9220));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 389, DateTimeKind.Local).AddTicks(9220));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 389, DateTimeKind.Local).AddTicks(9221));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 389, DateTimeKind.Local).AddTicks(9222));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 390, DateTimeKind.Local).AddTicks(1816));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 390, DateTimeKind.Local).AddTicks(1825));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 390, DateTimeKind.Local).AddTicks(1827));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 390, DateTimeKind.Local).AddTicks(1828));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(3980));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(3983));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(3984));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(3985));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(3986));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(3987));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(3988));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(3988));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(3989));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(3990));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(3991));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(3992));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(3993));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(3994));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(3995));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(4029));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(4030));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(4031));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(4032));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(4033));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(4033));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(4034));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(4035));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(4036));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(4037));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(4038));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(4039));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(4039));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(4040));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 385, DateTimeKind.Local).AddTicks(4041));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 386, DateTimeKind.Local).AddTicks(8784));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 386, DateTimeKind.Local).AddTicks(8796));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 386, DateTimeKind.Local).AddTicks(8800));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 11, 8, 59, 386, DateTimeKind.Local).AddTicks(8802));
        }
    }
}
