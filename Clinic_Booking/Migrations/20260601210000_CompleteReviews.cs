using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_Booking.Migrations
{
    public partial class CompleteReviews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_AppoinmentId",
                table: "Reviews");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Reviews",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_AppoinmentId",
                table: "Reviews",
                column: "AppoinmentId",
                unique: true,
                filter: "\"AppoinmentId\" IS NOT NULL AND \"IsDeleted\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reviews_Rating_Range",
                table: "Reviews",
                sql: "\"Rating\" >= 1 AND \"Rating\" <= 5");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_AppoinmentId",
                table: "Reviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reviews_Rating_Range",
                table: "Reviews");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                table: "Reviews",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_AppoinmentId",
                table: "Reviews",
                column: "AppoinmentId");
        }
    }
}
