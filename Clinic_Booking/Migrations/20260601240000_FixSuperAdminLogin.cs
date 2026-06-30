using Clinic_Booking.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_Booking.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260601240000_FixSuperAdminLogin")]
    public class FixSuperAdminLogin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "AspNetUsers"
                SET "NormalizedUserName" = 'SUPERADMIN'
                WHERE "UserName" = 'superadmin';

                INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
                SELECT users."Id", roles."Id"
                FROM "AspNetUsers" AS users
                CROSS JOIN "AspNetRoles" AS roles
                WHERE users."UserName" = 'superadmin'
                  AND roles."NormalizedName" = 'SUPERADMIN'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM "AspNetUserRoles" AS user_roles
                      WHERE user_roles."UserId" = users."Id"
                        AND user_roles."RoleId" = roles."Id"
                  );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "AspNetUserRoles"
                USING "AspNetUsers", "AspNetRoles"
                WHERE "AspNetUserRoles"."UserId" = "AspNetUsers"."Id"
                  AND "AspNetUserRoles"."RoleId" = "AspNetRoles"."Id"
                  AND "AspNetUsers"."UserName" = 'superadmin'
                  AND "AspNetRoles"."NormalizedName" = 'SUPERADMIN';

                UPDATE "AspNetUsers"
                SET "NormalizedUserName" = 'SUPARADMIN'
                WHERE "UserName" = 'superadmin';
                """);
        }
    }
}
