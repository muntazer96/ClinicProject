using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Clinic_Booking.Migrations
{
    /// <inheritdoc />
    public partial class CDR1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleterId = table.Column<int>(type: "integer", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifierId = table.Column<int>(type: "integer", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ImageName = table.Column<string>(type: "text", nullable: true),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    IsFirstLogin = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DeleterId = table.Column<int>(type: "integer", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ModifierId = table.Column<int>(type: "integer", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Days",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NormalizedName = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Days", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsPremiumOnly = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_Features", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Specializations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NormalizedName = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_Specializations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    YearlyPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxDailyAppointments = table.Column<int>(type: "integer", nullable: false),
                    MaxWeeklyDays = table.Column<int>(type: "integer", nullable: false),
                    ShowReviews = table.Column<bool>(type: "boolean", nullable: false),
                    ShowMessages = table.Column<bool>(type: "boolean", nullable: false),
                    EBooking = table.Column<bool>(type: "boolean", nullable: false),
                    EPayments = table.Column<bool>(type: "boolean", nullable: false),
                    MakeOffers = table.Column<bool>(type: "boolean", nullable: false),
                    MaxActiveOffers = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_SubscriptionPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NormalizedName = table.Column<string>(type: "text", nullable: false),
                    SpecializationId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    SubscriptionRank = table.Column<int>(type: "integer", nullable: false),
                    IraqiProvince = table.Column<int>(type: "integer", nullable: false),
                    ImageName = table.Column<string>(type: "text", nullable: false),
                    BirthDay = table.Column<DateOnly>(type: "date", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Doctors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doctors_Specializations_SpecializationId",
                        column: x => x.SpecializationId,
                        principalTable: "Specializations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    AppointmentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DoctorAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    DayId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    MaxAppointments = table.Column<int>(type: "integer", nullable: false),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_DoctorAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorAvailabilities_Days_DayId",
                        column: x => x.DayId,
                        principalTable: "Days",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorAvailabilities_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorFeature",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    FeatureId = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_DoctorFeature", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorFeature_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorFeature_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DoctorSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    PackageId = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                    table.PrimaryKey("PK_DoctorSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorSubscriptions_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorSubscriptions_SubscriptionPackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "SubscriptionPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiverId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Doctors_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Message = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoctorId = table.Column<int>(type: "integer", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Referrals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InviterDoctorId = table.Column<int>(type: "integer", nullable: false),
                    InvitedDoctorId = table.Column<int>(type: "integer", nullable: false),
                    ReferredAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
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
                    table.PrimaryKey("PK_Referrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Referrals_Doctors_InvitedDoctorId",
                        column: x => x.InvitedDoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Referrals_Doctors_InviterDoctorId",
                        column: x => x.InviterDoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AppointmentId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    AppoinmentId = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Appointments_AppoinmentId",
                        column: x => x.AppoinmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("090b126a-57e0-470e-9c0a-5daef229487a"), null, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8581), null, null, null, false, null, null, "SuperAdmin", "SUPERADMIN" },
                    { new Guid("0cf13926-eace-45be-8ee5-7d32b1ffc7d2"), null, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8594), null, null, null, false, null, null, "DoctorUser", "DOCTORUSER" },
                    { new Guid("60e0b2ce-1432-4e9d-a261-2d534bf0ebaf"), null, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8592), null, null, null, false, null, null, "NormalUser", "NORMALUSER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Email", "EmailConfirmed", "ImageName", "IsDeleted", "IsFirstLogin", "IsLocked", "LastLoginDate", "LockoutEnabled", "LockoutEnd", "ModifiedAt", "ModifierId", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("b9d7b525-b00f-447e-b50d-39fc00a5406b"), 0, "0d506b68-b10c-4490-be01-635ebe18aa0a", new DateTime(2025, 6, 8, 18, 47, 11, 249, DateTimeKind.Utc).AddTicks(3685), null, null, null, null, false, null, false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, null, null, null, null, "SUPARADMIN", "AQAAAAIAAYagAAAAELjaF/WDs9p8Hj6Xh7hwUIde3loNch3BldIkdBRW1GADNbzqQ41dxSBiokPTYfnHzg==", null, false, "0ae899e5-c533-40ae-98dd-5378fd8917d8", false, "superadmin" });

            migrationBuilder.InsertData(
                table: "Days",
                columns: new[] { "Id", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(3807), null, null, null, false, null, null, "السبت", "Saturday" },
                    { 2, new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(3812), null, null, null, false, null, null, "الاحد", "Sunday" },
                    { 3, new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(3813), null, null, null, false, null, null, "الاثنين", "Monday" },
                    { 4, new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(3814), null, null, null, false, null, null, "الثلاثاء", "Tuesday" },
                    { 5, new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(3814), null, null, null, false, null, null, "الاربعاء", "Wednesday" },
                    { 6, new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(3815), null, null, null, false, null, null, "الخميس", "Thursday" },
                    { 7, new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(3816), null, null, null, false, null, null, "الجمعة", "Friday" }
                });

            migrationBuilder.InsertData(
                table: "Features",
                columns: new[] { "Id", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "Description", "IsDeleted", "IsPremiumOnly", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(5801), null, null, null, "", false, true, null, null, "تفعيل التقييم والردود", "ShowReviews" },
                    { 2, new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(5807), null, null, null, "", false, true, null, null, "تفعيل زر الرسائل", "ShowMessages" },
                    { 3, new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(5808), null, null, null, "", false, true, null, null, "تفعيل الحجز الالكتروني", "EBooking" },
                    { 4, new DateTime(2025, 6, 8, 21, 47, 11, 306, DateTimeKind.Local).AddTicks(5809), null, null, null, "", false, true, null, null, "تفعيل الدفع الالكتروني", "EPayments" }
                });

            migrationBuilder.InsertData(
                table: "Specializations",
                columns: new[] { "Id", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "IsDeleted", "ModifiedAt", "ModifierId", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8952), null, null, null, false, null, null, "أخصائي باطنية", "Internal Medicine Specialist" },
                    { 2, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8955), null, null, null, false, null, null, "أخصائي أنف وأذن وحنجرة", "ENT Specialist (Ear, Nose, and Throat)" },
                    { 3, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8956), null, null, null, false, null, null, "أخصائي قلب", "Cardiologist" },
                    { 4, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8957), null, null, null, false, null, null, "أخصائي عيون", "Ophthalmologist" },
                    { 5, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8958), null, null, null, false, null, null, "أخصائي جلدية", "Dermatologist" },
                    { 6, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8959), null, null, null, false, null, null, "أخصائي أعصاب", "Neurologist" },
                    { 7, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8959), null, null, null, false, null, null, "أخصائي جراحة عامة", "General Surgeon" },
                    { 8, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8960), null, null, null, false, null, null, "أخصائي جراحة عظام", "Orthopedic Surgeon" },
                    { 9, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8961), null, null, null, false, null, null, "أخصائي نسائية وتوليد", "Obstetrician-Gynecologist (OB-GYN)" },
                    { 10, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8962), null, null, null, false, null, null, "أخصائي أطفال", "Pediatrician" },
                    { 11, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8963), null, null, null, false, null, null, "أخصائي أورام", "Oncologist" },
                    { 12, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8963), null, null, null, false, null, null, "أخصائي كلى", "Nephrologist" },
                    { 13, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8964), null, null, null, false, null, null, "أخصائي جهاز هضمي", "Gastroenterologist" },
                    { 14, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8965), null, null, null, false, null, null, "أخصائي غدد صماء", "Endocrinologist" },
                    { 15, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8965), null, null, null, false, null, null, "أخصائي جراحة تجميل", "Plastic Surgeon" },
                    { 16, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8966), null, null, null, false, null, null, "أخصائي جراحة دماغ وأعصاب", "Neurosurgeon" },
                    { 17, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8967), null, null, null, false, null, null, "أخصائي تخدير", "Anesthesiologist" },
                    { 18, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8968), null, null, null, false, null, null, "أخصائي طب الأسرة", "Family Medicine Specialist" },
                    { 19, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8968), null, null, null, false, null, null, "أخصائي الطب النفسي", "Psychiatrist" },
                    { 20, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8969), null, null, null, false, null, null, "أخصائي أمراض معدية", "Infectious Disease Specialist" },
                    { 21, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8970), null, null, null, false, null, null, "أخصائي أشعة", "Radiologist" },
                    { 22, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8970), null, null, null, false, null, null, "أخصائي طب طوارئ", "Emergency Medicine Specialist" },
                    { 23, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8971), null, null, null, false, null, null, "أخصائي روماتيزم", "Rheumatologist" },
                    { 24, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8972), null, null, null, false, null, null, "أخصائي صدرية", "Pulmonologist" },
                    { 25, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8972), null, null, null, false, null, null, "أخصائي طب مهني", "Occupational Medicine Specialist" },
                    { 26, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8973), null, null, null, false, null, null, "أخصائي طب رياضي", "Sports Medicine Specialist" },
                    { 27, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8974), null, null, null, false, null, null, "أخصائي أمراض الدم", "Hematologist" },
                    { 28, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8975), null, null, null, false, null, null, "أخصائي علاج طبيعي", "Physiotherapist" },
                    { 29, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8976), null, null, null, false, null, null, "أخصائي تغذية", "Nutritionist" },
                    { 30, new DateTime(2025, 6, 8, 21, 47, 11, 303, DateTimeKind.Local).AddTicks(8977), null, null, null, false, null, null, "أخصائي نطق وتخاطب", "Speech Therapist" }
                });

            migrationBuilder.InsertData(
                table: "SubscriptionPackages",
                columns: new[] { "Id", "CreatedAt", "CreatorId", "DeletedAt", "DeleterId", "EBooking", "EPayments", "IsDeleted", "MakeOffers", "MaxActiveOffers", "MaxDailyAppointments", "MaxWeeklyDays", "ModifiedAt", "ModifierId", "Name", "NormalizedName", "Price", "ShowMessages", "ShowReviews", "YearlyPrice" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 6, 8, 21, 47, 11, 304, DateTimeKind.Local).AddTicks(7547), null, null, null, false, false, false, false, 0, 15, 4, null, null, "أساسي", "Basic", 0m, false, false, 0m },
                    { 2, new DateTime(2025, 6, 8, 21, 47, 11, 304, DateTimeKind.Local).AddTicks(7555), null, null, null, false, false, false, false, 0, 25, 5, null, null, "ذهبي", "Gold", 25m, false, true, 250m },
                    { 3, new DateTime(2025, 6, 8, 21, 47, 11, 304, DateTimeKind.Local).AddTicks(7560), null, null, null, true, true, false, true, 1, 35, 6, null, null, "ألماس", "Diamond ", 35m, true, true, 350m },
                    { 4, new DateTime(2025, 6, 8, 21, 47, 11, 304, DateTimeKind.Local).AddTicks(7562), null, null, null, true, true, false, true, 2, 1000, 7, null, null, "فاخر", "Premium", 45m, true, true, 450m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DoctorId",
                table: "Appointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_UserId",
                table: "Appointments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilities_DayId",
                table: "DoctorAvailabilities",
                column: "DayId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorAvailabilities_DoctorId",
                table: "DoctorAvailabilities",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorFeature_DoctorId",
                table: "DoctorFeature",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorFeature_FeatureId",
                table: "DoctorFeature",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_SpecializationId",
                table: "Doctors",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSubscriptions_DoctorId",
                table: "DoctorSubscriptions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorSubscriptions_PackageId",
                table: "DoctorSubscriptions",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverId",
                table: "Messages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DoctorId",
                table: "Notifications",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_AppointmentId",
                table: "Payments",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_InvitedDoctorId",
                table: "Referrals",
                column: "InvitedDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_InviterDoctorId",
                table: "Referrals",
                column: "InviterDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_AppoinmentId",
                table: "Reviews",
                column: "AppoinmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_DoctorId",
                table: "Reviews",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "DoctorAvailabilities");

            migrationBuilder.DropTable(
                name: "DoctorFeature");

            migrationBuilder.DropTable(
                name: "DoctorSubscriptions");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Referrals");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Days");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "SubscriptionPackages");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "Specializations");
        }
    }
}
