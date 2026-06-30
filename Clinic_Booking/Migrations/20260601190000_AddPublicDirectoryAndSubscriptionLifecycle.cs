using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_Booking.Migrations
{
    public partial class AddPublicDirectoryAndSubscriptionLifecycle : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorSubscriptions_DoctorId",
                table: "DoctorSubscriptions");

            migrationBuilder.AddColumn<bool>(
                name: "IsPubliclyVisible",
                table: "Doctors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxClinics",
                table: "SubscriptionPackages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "DoctorSubscriptions",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "DoctorSubscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "MaxClinics",
                value: 1);

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "MaxClinics",
                value: 2);

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                column: "MaxClinics",
                value: 3);

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "MaxClinics",
                value: 5);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_IsPubliclyVisible_SpecializationId",
                table: "Doctors",
                columns: new[] { "IsPubliclyVisible", "SpecializationId" });

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_NormalizedName",
                table: "Doctors",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_IraqiProvince_IsVisible_IsDeleted",
                table: "Clinics",
                columns: new[] { "IraqiProvince", "IsVisible", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSubscriptions_DoctorId_Status_EndDate",
                table: "DoctorSubscriptions",
                columns: new[] { "DoctorId", "Status", "EndDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Doctors_IsPubliclyVisible_SpecializationId",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_NormalizedName",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_Clinics_IraqiProvince_IsVisible_IsDeleted",
                table: "Clinics");

            migrationBuilder.DropIndex(
                name: "IX_DoctorSubscriptions_DoctorId_Status_EndDate",
                table: "DoctorSubscriptions");

            migrationBuilder.DropColumn(
                name: "IsPubliclyVisible",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "MaxClinics",
                table: "SubscriptionPackages");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "DoctorSubscriptions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "DoctorSubscriptions");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSubscriptions_DoctorId",
                table: "DoctorSubscriptions",
                column: "DoctorId");
        }
    }
}
