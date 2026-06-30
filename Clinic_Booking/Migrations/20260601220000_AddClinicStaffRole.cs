using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_Booking.Migrations
{
    public partial class AddClinicStaffRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "IsDeleted", "Name", "NormalizedName" },
                values: new object[]
                {
                    new Guid("6f03ef0f-c1ac-43f6-9df2-2be6d5385b72"),
                    false,
                    "ClinicStaff",
                    "CLINICSTAFF"
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("6f03ef0f-c1ac-43f6-9df2-2be6d5385b72"));
        }
    }
}
