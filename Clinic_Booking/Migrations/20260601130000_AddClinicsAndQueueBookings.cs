using System;
using Clinic_Booking.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Clinic_Booking.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260601130000_AddClinicsAndQueueBookings")]
    public class AddClinicsAndQueueBookings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clinics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IraqiProvince = table.Column<int>(type: "integer", nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    MapUrl = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Clinics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clinics_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_DoctorId",
                table: "Clinics",
                column: "DoctorId");

            migrationBuilder.Sql(
                """
                INSERT INTO "Clinics"
                    ("DoctorId", "Name", "IraqiProvince", "Address", "PhoneNumber", "IsVisible", "IsDeleted", "CreatedAt")
                SELECT
                    "Id", "Name", "IraqiProvince", COALESCE("Location", ''), "PhoneNumber", TRUE, FALSE, NOW()
                FROM "Doctors";
                """);

            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "DoctorAvailabilities",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "DoctorAvailabilities" AS availability
                SET "ClinicId" = clinic."Id"
                FROM "Clinics" AS clinic
                WHERE clinic."DoctorId" = availability."DoctorId";
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAvailabilities_Doctors_DoctorId",
                table: "DoctorAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_DoctorAvailabilities_DoctorId",
                table: "DoctorAvailabilities");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "DoctorAvailabilities");

            migrationBuilder.AlterColumn<int>(
                name: "ClinicId",
                table: "DoctorAvailabilities",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilities_ClinicId",
                table: "DoctorAvailabilities",
                column: "ClinicId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAvailabilities_Clinics_ClinicId",
                table: "DoctorAvailabilities",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Appointments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "Appointments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QueueNumber",
                table: "Appointments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "GuestName",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestPhoneNumber",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPhoneConfirmed",
                table: "Appointments",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Appointments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CancelledByUserId",
                table: "Appointments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Appointments",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "Appointments" AS appointment
                SET "ClinicId" = clinic."Id"
                FROM "Clinics" AS clinic
                WHERE clinic."DoctorId" = appointment."DoctorId";

                WITH numbered AS (
                    SELECT
                        "Id",
                        ROW_NUMBER() OVER (
                            PARTITION BY "ClinicId", CAST("AppointmentDate" AS date)
                            ORDER BY "AppointmentDate", "Id"
                        ) AS queue_number
                    FROM "Appointments"
                )
                UPDATE "Appointments" AS appointment
                SET "QueueNumber" = numbered.queue_number
                FROM numbered
                WHERE appointment."Id" = numbered."Id";
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ClinicId",
                table: "Appointments",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ClinicId_AppointmentDate_QueueNumber",
                table: "Appointments",
                columns: new[] { "ClinicId", "AppointmentDate", "QueueNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Clinics_ClinicId",
                table: "Appointments",
                column: "ClinicId",
                principalTable: "Clinics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Clinics_ClinicId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorAvailabilities_Clinics_ClinicId",
                table: "DoctorAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ClinicId_AppointmentDate_QueueNumber",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_DoctorAvailabilities_ClinicId",
                table: "DoctorAvailabilities");

            migrationBuilder.AddColumn<int>(
                name: "DoctorId",
                table: "DoctorAvailabilities",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE "DoctorAvailabilities" AS availability
                SET "DoctorId" = clinic."DoctorId"
                FROM "Clinics" AS clinic
                WHERE clinic."Id" = availability."ClinicId";
                """);

            migrationBuilder.AlterColumn<int>(
                name: "DoctorId",
                table: "DoctorAvailabilities",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.DropColumn(name: "ClinicId", table: "DoctorAvailabilities");
            migrationBuilder.DropColumn(name: "ClinicId", table: "Appointments");
            migrationBuilder.DropColumn(name: "QueueNumber", table: "Appointments");
            migrationBuilder.DropColumn(name: "GuestName", table: "Appointments");
            migrationBuilder.DropColumn(name: "GuestPhoneNumber", table: "Appointments");
            migrationBuilder.DropColumn(name: "Notes", table: "Appointments");
            migrationBuilder.DropColumn(name: "IsPhoneConfirmed", table: "Appointments");
            migrationBuilder.DropColumn(name: "CancellationReason", table: "Appointments");
            migrationBuilder.DropColumn(name: "CancelledByUserId", table: "Appointments");
            migrationBuilder.DropColumn(name: "CancelledAt", table: "Appointments");

            migrationBuilder.Sql("""DELETE FROM "Appointments" WHERE "UserId" IS NULL;""");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Appointments",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilities_DoctorId",
                table: "DoctorAvailabilities",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorAvailabilities_Doctors_DoctorId",
                table: "DoctorAvailabilities",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropTable(name: "Clinics");
        }
    }
}
