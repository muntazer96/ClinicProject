using System;
using Clinic_Booking.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Clinic_Booking.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260601150000_AddBookingOtpRequests")]
    public class AddBookingOtpRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingOtpRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppointmentId = table.Column<int>(type: "integer", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CodeSalt = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
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
                    table.PrimaryKey("PK_BookingOtpRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingOtpRequests_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingOtpRequests_AppointmentId_IsUsed",
                table: "BookingOtpRequests",
                columns: new[] { "AppointmentId", "IsUsed" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BookingOtpRequests");
        }
    }
}
