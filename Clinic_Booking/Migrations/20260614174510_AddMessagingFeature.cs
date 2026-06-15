using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class AddMessagingFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorSubscriptions_Doctors_DoctorId",
                table: "DoctorSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorSubscriptions_SubscriptionPackages_PackageId",
                table: "DoctorSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Doctors_ReceiverId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Appointments_AppointmentId",
                table: "Payments");

            migrationBuilder.AddColumn<Guid>(
                name: "ReceiverUserId",
                table: "Messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Messages" AS m
                SET "ReceiverUserId" = d."UserId"
                FROM "Doctors" AS d
                WHERE m."ReceiverId" = d."Id";
                """);

            migrationBuilder.Sql("""
                DELETE FROM "Messages"
                WHERE "ReceiverUserId" IS NULL;
                """);

            migrationBuilder.DropColumn(
                name: "ReceiverId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "ReceiverUserId",
                table: "Messages",
                newName: "ReceiverId");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReceiverId",
                table: "Messages",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverId",
                table: "Messages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId_ReceiverId_SentAt",
                table: "Messages",
                columns: new[] { "SenderId", "ReceiverId", "SentAt" });

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "Messages",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverId_IsRead",
                table: "Messages",
                columns: new[] { "ReceiverId", "IsRead" });

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Appointments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("5ca230af-a98f-4d9e-b99d-0b58d33a4379"),
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 12, 53, 15, 88, DateTimeKind.Utc).AddTicks(7335));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("6f03ef0f-c1ac-43f6-9df2-2be6d5385b72"),
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 12, 53, 15, 88, DateTimeKind.Utc).AddTicks(7347));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("99f94c2e-be3b-4f90-b59f-81ac294980bf"),
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 12, 53, 15, 88, DateTimeKind.Utc).AddTicks(7338));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("f6efb588-1fd1-4df4-a453-eb11f69f9046"),
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 12, 53, 15, 88, DateTimeKind.Utc).AddTicks(7316));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("db3946f4-275b-4bc1-ac3a-a6e1f3f4badb"),
                column: "ConcurrencyStamp",
                value: "7f835d58-1359-4901-a66f-7f7f4174c45f");

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 980, DateTimeKind.Utc).AddTicks(284));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 980, DateTimeKind.Utc).AddTicks(294));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 980, DateTimeKind.Utc).AddTicks(295));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 980, DateTimeKind.Utc).AddTicks(295));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 980, DateTimeKind.Utc).AddTicks(296));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 980, DateTimeKind.Utc).AddTicks(297));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 980, DateTimeKind.Utc).AddTicks(298));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 980, DateTimeKind.Utc).AddTicks(3335));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 980, DateTimeKind.Utc).AddTicks(3339));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 980, DateTimeKind.Utc).AddTicks(3340));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 980, DateTimeKind.Utc).AddTicks(3341));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7202));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7206));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7207));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7208));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7209));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7209));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7211));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7212));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7212));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7213));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7214));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7215));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7216));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7216));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7218));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7219));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7219));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7220));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7221));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7222));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7222));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7223));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7224));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7225));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7226));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7227));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7228));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7229));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7229));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 967, DateTimeKind.Utc).AddTicks(7230));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 975, DateTimeKind.Utc).AddTicks(8752));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 975, DateTimeKind.Utc).AddTicks(8765));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "NormalizedName" },
                values: new object[] { new DateTime(2026, 6, 14, 17, 45, 8, 975, DateTimeKind.Utc).AddTicks(8770), "Diamond" });

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 14, 17, 45, 8, 975, DateTimeKind.Utc).AddTicks(8772));

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorSubscriptions_Doctors_DoctorId",
                table: "DoctorSubscriptions",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorSubscriptions_SubscriptionPackages_PackageId",
                table: "DoctorSubscriptions",
                column: "PackageId",
                principalTable: "SubscriptionPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_AspNetUsers_ReceiverId",
                table: "Messages",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Appointments_AppointmentId",
                table: "Payments",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorSubscriptions_Doctors_DoctorId",
                table: "DoctorSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorSubscriptions_SubscriptionPackages_PackageId",
                table: "DoctorSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_AspNetUsers_ReceiverId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Appointments_AppointmentId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ReceiverId_IsRead",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderId_ReceiverId_SentAt",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ReceiverId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "Messages");

            migrationBuilder.AlterColumn<int>(
                name: "ReceiverId",
                table: "Messages",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Appointments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("5ca230af-a98f-4d9e-b99d-0b58d33a4379"),
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 15, 53, 15, 88, DateTimeKind.Local).AddTicks(7335));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("6f03ef0f-c1ac-43f6-9df2-2be6d5385b72"),
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 15, 53, 15, 88, DateTimeKind.Local).AddTicks(7347));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("99f94c2e-be3b-4f90-b59f-81ac294980bf"),
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 15, 53, 15, 88, DateTimeKind.Local).AddTicks(7338));

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("f6efb588-1fd1-4df4-a453-eb11f69f9046"),
                column: "CreatedAt",
                value: new DateTime(2026, 6, 4, 15, 53, 15, 88, DateTimeKind.Local).AddTicks(7316));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("db3946f4-275b-4bc1-ac3a-a6e1f3f4badb"),
                column: "ConcurrencyStamp",
                value: "3e733150-2edb-4cf8-b807-fda7cf2b618f");

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 344, DateTimeKind.Local).AddTicks(2239));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 344, DateTimeKind.Local).AddTicks(2249));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 344, DateTimeKind.Local).AddTicks(2250));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 344, DateTimeKind.Local).AddTicks(2251));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 344, DateTimeKind.Local).AddTicks(2252));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 344, DateTimeKind.Local).AddTicks(2253));

            migrationBuilder.UpdateData(
                table: "Days",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 344, DateTimeKind.Local).AddTicks(2254));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 344, DateTimeKind.Local).AddTicks(3137));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 344, DateTimeKind.Local).AddTicks(3140));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 344, DateTimeKind.Local).AddTicks(3141));

            migrationBuilder.UpdateData(
                table: "Features",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 344, DateTimeKind.Local).AddTicks(3142));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2559));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2562));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2563));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2564));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2565));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2566));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2567));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2568));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2568));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2569));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2570));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2571));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2572));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2573));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2573));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2574));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2575));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2576));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2577));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2578));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2578));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2579));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2580));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2581));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2582));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2582));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2583));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 28,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2584));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 29,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2585));

            migrationBuilder.UpdateData(
                table: "Specializations",
                keyColumn: "Id",
                keyValue: 30,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(2586));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(8691));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(8705));

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "NormalizedName" },
                values: new object[] { new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(8710), "Diamond " });

            migrationBuilder.UpdateData(
                table: "SubscriptionPackages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 6, 13, 8, 58, 14, 342, DateTimeKind.Local).AddTicks(8712));

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorSubscriptions_Doctors_DoctorId",
                table: "DoctorSubscriptions",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorSubscriptions_SubscriptionPackages_PackageId",
                table: "DoctorSubscriptions",
                column: "PackageId",
                principalTable: "SubscriptionPackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Doctors_ReceiverId",
                table: "Messages",
                column: "ReceiverId",
                principalTable: "Doctors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Appointments_AppointmentId",
                table: "Payments",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
