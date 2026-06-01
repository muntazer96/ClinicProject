using Clinic_Booking.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_Booking.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260601230000_AddAppointmentLookupIndexes")]
    public class AddAppointmentLookupIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_UserId",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ClinicId_AppointmentDate_Status",
                table: "Appointments",
                columns: new[] { "ClinicId", "AppointmentDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_GuestPhoneNumber_AppointmentDate_Status",
                table: "Appointments",
                columns: new[] { "GuestPhoneNumber", "AppointmentDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_UserId_AppointmentDate_Status",
                table: "Appointments",
                columns: new[] { "UserId", "AppointmentDate", "Status" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_ClinicId_AppointmentDate_Status",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_GuestPhoneNumber_AppointmentDate_Status",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_UserId_AppointmentDate_Status",
                table: "Appointments");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_UserId",
                table: "Appointments",
                column: "UserId");
        }
    }
}
