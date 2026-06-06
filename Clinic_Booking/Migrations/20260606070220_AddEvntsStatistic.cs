using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class AddEvntsStatistic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalyticsEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventType = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsGuest = table.Column<bool>(type: "boolean", nullable: false),
                    DoctorId = table.Column<int>(type: "integer", nullable: true),
                    ClinicId = table.Column<int>(type: "integer", nullable: true),
                    SpecializationId = table.Column<int>(type: "integer", nullable: true),
                    AppointmentId = table.Column<int>(type: "integer", nullable: true),
                    OfferId = table.Column<int>(type: "integer", nullable: true),
                    Source = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Platform = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Page = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Province = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    SearchText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SessionId = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                    table.PrimaryKey("PK_AnalyticsEvents", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("db3946f4-275b-4bc1-ac3a-a6e1f3f4badb"),
                column: "ConcurrencyStamp",
                value: "67c5dec3-0c0f-44a2-a18f-c64ca7591aa5");

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 287, DateTimeKind.Local).AddTicks(9973));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 287, DateTimeKind.Local).AddTicks(9983));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 287, DateTimeKind.Local).AddTicks(9985));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 287, DateTimeKind.Local).AddTicks(9986));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 287, DateTimeKind.Local).AddTicks(9986));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 287, DateTimeKind.Local).AddTicks(9987));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 287, DateTimeKind.Local).AddTicks(9988));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 288, DateTimeKind.Local).AddTicks(941));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 288, DateTimeKind.Local).AddTicks(945));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 288, DateTimeKind.Local).AddTicks(946));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 288, DateTimeKind.Local).AddTicks(947));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9411));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9414));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9415));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9416));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9417));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9418));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9419));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9420));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9421));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9422));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9423));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9424));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9448));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9449));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9450));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9451));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9452));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9453));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9454));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9455));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9455));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9456));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9457));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9458));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9459));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9460));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9461));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9462));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9462));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 285, DateTimeKind.Local).AddTicks(9463));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 286, DateTimeKind.Local).AddTicks(5499));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 286, DateTimeKind.Local).AddTicks(5511));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 286, DateTimeKind.Local).AddTicks(5516));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 6, 10, 2, 20, 286, DateTimeKind.Local).AddTicks(5518));

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_DoctorId_EventType_OccurredAt",
                table: "AnalyticsEvents",
                columns: new[] { "DoctorId", "EventType", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_EventType_OccurredAt",
                table: "AnalyticsEvents",
                columns: new[] { "EventType", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_SessionId_OccurredAt",
                table: "AnalyticsEvents",
                columns: new[] { "SessionId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsEvents_UserId_OccurredAt",
                table: "AnalyticsEvents",
                columns: new[] { "UserId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsEvents");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("db3946f4-275b-4bc1-ac3a-a6e1f3f4badb"),
                column: "ConcurrencyStamp",
                value: "44d3a466-86a7-4d33-adc0-219363d41a58");

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 446, DateTimeKind.Local).AddTicks(3994));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 446, DateTimeKind.Local).AddTicks(4013));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 446, DateTimeKind.Local).AddTicks(4014));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 446, DateTimeKind.Local).AddTicks(4015));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 446, DateTimeKind.Local).AddTicks(4016));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 446, DateTimeKind.Local).AddTicks(4017));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 446, DateTimeKind.Local).AddTicks(4018));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 446, DateTimeKind.Local).AddTicks(7082));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 446, DateTimeKind.Local).AddTicks(7092));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 446, DateTimeKind.Local).AddTicks(7094));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 446, DateTimeKind.Local).AddTicks(7095));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(1941));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(1950));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(1952));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(1953));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(1955));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(1956));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(1957));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(1959));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(1960));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(1962));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(1963));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(1964));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(1966));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2030));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2032));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2033));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2035));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2036));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2037));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2039));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2040));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2041));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2043));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2045));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2046));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2047));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2048));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2049));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2051));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 440, DateTimeKind.Local).AddTicks(2052));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 442, DateTimeKind.Local).AddTicks(5418));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 442, DateTimeKind.Local).AddTicks(5438));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 442, DateTimeKind.Local).AddTicks(5445));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 5, 20, 28, 29, 442, DateTimeKind.Local).AddTicks(5447));
        }
    }
}
