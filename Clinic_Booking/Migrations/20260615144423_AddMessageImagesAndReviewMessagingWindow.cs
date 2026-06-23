using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageImagesAndReviewMessagingWindow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Messages"
                ADD COLUMN IF NOT EXISTS "ImageName" character varying(260);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Messages"
                DROP COLUMN IF EXISTS "ImageName";
                """);
        }
    }
}
