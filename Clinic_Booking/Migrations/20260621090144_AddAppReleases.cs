using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class AddAppReleases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppReleases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VersionName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    VersionCode = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ReleaseNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DownloadCount = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_AppReleases", x => x.Id);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_AppReleases_IsActive_IsDeleted",
                table: "AppReleases",
                columns: new[] { "IsActive", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppReleases");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("db3946f4-275b-4bc1-ac3a-a6e1f3f4badb"),
                column: "ConcurrencyStamp",
                value: "563332d3-51e6-4195-b510-ceb78369dfb7");

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(5386));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(5403));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(5405));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(5406));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(5408));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(5409));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(5410));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(6214));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(6219));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(6221));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 591, DateTimeKind.Unspecified).AddTicks(6222));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8083));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8087));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8088));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8090));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8091));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8092));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8093));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8095));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8096));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8098));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8099));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8100));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8101));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8102));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8132));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8134));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8136));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8137));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8139));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8140));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8141));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8142));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8143));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8148));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8149));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8151));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8152));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8153));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8154));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 589, DateTimeKind.Unspecified).AddTicks(8156));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 590, DateTimeKind.Unspecified).AddTicks(3419));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 590, DateTimeKind.Unspecified).AddTicks(3436));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 590, DateTimeKind.Unspecified).AddTicks(3441));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 21, 11, 45, 22, 590, DateTimeKind.Unspecified).AddTicks(3444));
        }
    }
}
