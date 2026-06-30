using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecializationIconName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IconName",
                table: "Specializations",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "medical-services");

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "IconName",
                value: "internal-medicine");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "IconName",
                value: "ent");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "IconName",
                value: "cardiology");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "IconName",
                value: "ophthalmology");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "IconName",
                value: "dermatology");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "IconName",
                value: "neurology");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "IconName",
                value: "general-surgery");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "IconName",
                value: "orthopedics");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "IconName",
                value: "gynecology");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "IconName",
                value: "pediatrics");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "IconName",
                value: "oncology");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "IconName",
                value: "nephrology");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "IconName",
                value: "gastroenterology");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "IconName",
                value: "endocrinology");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "IconName",
                value: "plastic-surgery");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "IconName",
                value: "neurosurgery");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "IconName",
                value: "anesthesiology");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "IconName",
                value: "family-medicine");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "IconName",
                value: "psychiatry");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "IconName",
                value: "infectious-disease");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "IconName",
                value: "radiology");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "IconName",
                value: "emergency");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "IconName",
                value: "rheumatology");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "IconName",
                value: "pulmonology");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "IconName",
                value: "occupational-medicine");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "IconName",
                value: "sports-medicine");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "IconName",
                value: "hematology");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "IconName",
                value: "physiotherapy");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "IconName",
                value: "nutrition");
            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "IconName",
                value: "speech-therapy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IconName",
                table: "Specializations");
        }
    }
}
