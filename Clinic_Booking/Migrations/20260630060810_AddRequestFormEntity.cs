using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestFormEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    KnownName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IraqiProvince = table.Column<int>(type: "integer", nullable: false),
                    BirthDay = table.Column<DateOnly>(type: "date", nullable: false),
                    SpecializationId = table.Column<int>(type: "integer", nullable: false),
                    IdentityFront = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IdentityBack = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequestStatus = table.Column<int>(type: "integer", nullable: false),
                    RejectedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_RequestForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestForms_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequestForms_Specializations_SpecializationId",
                        column: x => x.SpecializationId,
                        principalTable: "Specializations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("db3946f4-275b-4bc1-ac3a-a6e1f3f4badb"),
                column: "ConcurrencyStamp",
                value: "097484ee-36cb-486b-8135-7e7fa1d027c7");

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(911));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(928));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(930));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(931));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(932));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(933));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(934));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(1877));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(1916));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(1918));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 243, DateTimeKind.Unspecified).AddTicks(1919));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(227));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(231));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(233));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(234));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(235));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(237));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(238));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(239));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(240));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(242));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(243));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(244));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(245));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(246));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(247));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(285));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(287));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(288));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(289));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(291));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(292));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(293));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(294));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(295));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(296));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(298));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(299));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(300));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(301));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(302));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(303));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(304));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(305));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(307));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(308));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(309));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(310));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(311));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(312));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(314));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(315));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(316));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(317));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(318));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(6622));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(6638));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(6644));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 30, 9, 8, 10, 241, DateTimeKind.Unspecified).AddTicks(6647));

            migrationBuilder.CreateIndex(
                name: "IX_RequestForms_Code",
                table: "RequestForms",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RequestForms_RequestStatus_IsDeleted",
                table: "RequestForms",
                columns: new[] { "RequestStatus", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_RequestForms_SpecializationId",
                table: "RequestForms",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestForms_UserId_RequestStatus_IsDeleted",
                table: "RequestForms",
                columns: new[] { "UserId", "RequestStatus", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestForms");

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

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 31,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3196));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 32,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3197));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 33,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3199));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 34,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3200));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 35,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3201));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 36,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3203));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 37,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3204));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 38,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3205));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 39,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3207));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 40,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3208));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 41,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3209));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 42,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3211));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 43,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3212));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 44,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 25, 23, 53, 34, 613, DateTimeKind.Unspecified).AddTicks(3213));

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
    }
}
