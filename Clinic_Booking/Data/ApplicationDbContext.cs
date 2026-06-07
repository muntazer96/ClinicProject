using Clinic_Booking.Entities.Appointment;
using Clinic_Booking.Entities.AppVersion;
using Clinic_Booking.Entities.Analytics;
using Clinic_Booking.Entities.Clinic;
using Clinic_Booking.Entities.ClinicException;
using Clinic_Booking.Entities.BookingOtpRequest;
using Clinic_Booking.Entities.Day;
using Clinic_Booking.Entities.Doctor;
using Clinic_Booking.Entities.DoctorAvailability;
using Clinic_Booking.Entities.DoctorFeature;
using Clinic_Booking.Entities.DoctorOffer;
using Clinic_Booking.Entities.DoctorSubscription;
using Clinic_Booking.Entities.DeviceToken;
using Clinic_Booking.Entities.Feature;
using Clinic_Booking.Entities.Message;
using Clinic_Booking.Entities.Notification;
using Clinic_Booking.Entities.Payment;
using Clinic_Booking.Entities.Referral;
using Clinic_Booking.Entities.RefreshToken;
using Clinic_Booking.Entities.Review;
using Clinic_Booking.Entities.Role;
using Clinic_Booking.Entities.Specialization;
using Clinic_Booking.Entities.SubscriptionPackage;
using Clinic_Booking.Entities.User;
using Clinic_Booking.Entities.UserPhoneOtpRequest;
using Clinic_Booking.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Data
{
    public class ApplicationDbContext : IdentityDbContext<AspNetUsers, AspNetRoles, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
        }

        // الجداول
        public DbSet<AspNetUsers> AspNetUsers { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<ClinicException> ClinicExceptions { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<SubscriptionPackage> SubscriptionPackages { get; set; }
        public DbSet<DoctorSubscription> DoctorSubscriptions { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<BookingOtpRequest> BookingOtpRequests { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<DeviceToken> DeviceTokens { get; set; }
        public DbSet<UserPhoneOtpRequest> UserPhoneOtpRequests { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<Day> Days { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<DoctorFeature> DoctorFeature { get; set; }
        public DbSet<DoctorOffer> DoctorOffers { get; set; }
        public DbSet<AppVersionPolicy> AppVersionPolicies { get; set; }
        public DbSet<AnalyticsEvent> AnalyticsEvents { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AnalyticsEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(80);
                entity.Property(e => e.Source).HasMaxLength(80);
                entity.Property(e => e.Platform).HasMaxLength(40);
                entity.Property(e => e.Page).HasMaxLength(120);
                entity.Property(e => e.Province).HasMaxLength(80);
                entity.Property(e => e.SearchText).HasMaxLength(300);
                entity.Property(e => e.SessionId).HasMaxLength(120);
                entity.HasIndex(e => new { e.EventType, e.OccurredAt });
                entity.HasIndex(e => new { e.DoctorId, e.EventType, e.OccurredAt });
                entity.HasIndex(e => new { e.UserId, e.OccurredAt });
                entity.HasIndex(e => new { e.SessionId, e.OccurredAt });
            });

            modelBuilder.Entity<AppVersionPolicy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Platform).IsUnique();
                entity.Property(e => e.Platform).IsRequired().HasMaxLength(40);
                entity.Property(e => e.LatestVersion).IsRequired().HasMaxLength(30);
                entity.Property(e => e.MinimumSupportedVersion).IsRequired().HasMaxLength(30);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(120);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(600);
                entity.Property(e => e.UpdateUrl).HasMaxLength(500);
            });

            // DoctorOffer
            modelBuilder.Entity<DoctorOffer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(160);
                entity.Property(e => e.Description).HasMaxLength(800);
                entity.Property(e => e.BadgeText).HasMaxLength(40);
                entity.Property(e => e.Terms).HasMaxLength(600);
                entity.Property(e => e.OriginalPrice).HasPrecision(10, 2);
                entity.Property(e => e.OfferPrice).HasPrecision(10, 2);
                entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);

                entity.HasIndex(e => new { e.DoctorId, e.IsActive, e.EndsAt });
                entity.HasIndex(e => new { e.ClinicId, e.IsDeleted });

                entity.HasOne(e => e.Doctor)
                    .WithMany()
                    .HasForeignKey(e => e.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Clinic)
                    .WithMany()
                    .HasForeignKey(e => e.ClinicId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // User
            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);


                entity.HasData(new Entities.User.AspNetUsers()
                {
                    Id = Guid.Parse("db3946f4-275b-4bc1-ac3a-a6e1f3f4badb"),
                    CreatedAt = new DateTime(2026, 6, 4, 12, 53, 15, 30, DateTimeKind.Utc).AddTicks(7963),
                    NormalizedUserName = "SUPERADMIN",
                    PasswordHash = "AQAAAAIAAYagAAAAEDoyZr7jFYw+dhzSmNW6K5jP/J6IiwtTB3xhO0utvtAPVvUYjIoXdfAFhW0FNbvKgQ==",
                    SecurityStamp = "86523971-50c4-48c6-bd51-d95f235847dc",
                    PhoneNumber = null,
                    UserName = "superadmin",
                });
            });

            //Role
            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasData(new AspNetRoles()
                {
                    Id = Guid.Parse("f6efb588-1fd1-4df4-a453-eb11f69f9046"),
                    CreatedAt = new DateTime(2026, 6, 4, 15, 53, 15, 88, DateTimeKind.Local).AddTicks(7316),
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN",
                }, new AspNetRoles()
                {
                    Id = Guid.Parse("5ca230af-a98f-4d9e-b99d-0b58d33a4379"),
                    CreatedAt = new DateTime(2026, 6, 4, 15, 53, 15, 88, DateTimeKind.Local).AddTicks(7335),
                    Name = "NormalUser",
                    NormalizedName = "NORMALUSER",
                }, new AspNetRoles()
                {
                    Id = Guid.Parse("99f94c2e-be3b-4f90-b59f-81ac294980bf"),
                    CreatedAt = new DateTime(2026, 6, 4, 15, 53, 15, 88, DateTimeKind.Local).AddTicks(7338),
                    Name = "DoctorUser",
                    NormalizedName = "DOCTORUSER",
                }, new AspNetRoles()
                {
                    Id = Guid.Parse("6f03ef0f-c1ac-43f6-9df2-2be6d5385b72"),
                    CreatedAt = new DateTime(2026, 6, 4, 15, 53, 15, 88, DateTimeKind.Local).AddTicks(7347),
                    Name = AppRoles.ClinicStaff,
                    NormalizedName = "CLINICSTAFF",
                });
            });

            // Specialization
            modelBuilder.Entity<Specialization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.IconName).IsRequired().HasMaxLength(80);
                entity.HasData(
    new Specialization { Id = 1, Name = "أخصائي باطنية", NormalizedName = "Internal Medicine Specialist", IconName = "internal-medicine" },
    new Specialization { Id = 2, Name = "أخصائي أنف وأذن وحنجرة", NormalizedName = "ENT Specialist (Ear, Nose, and Throat)", IconName = "ent" },
    new Specialization { Id = 3, Name = "أخصائي قلب", NormalizedName = "Cardiologist", IconName = "cardiology" },
    new Specialization { Id = 4, Name = "أخصائي عيون", NormalizedName = "Ophthalmologist", IconName = "ophthalmology" },
    new Specialization { Id = 5, Name = "أخصائي جلدية", NormalizedName = "Dermatologist", IconName = "dermatology" },
    new Specialization { Id = 6, Name = "أخصائي أعصاب", NormalizedName = "Neurologist", IconName = "neurology" },
    new Specialization { Id = 7, Name = "أخصائي جراحة عامة", NormalizedName = "General Surgeon", IconName = "general-surgery" },
    new Specialization { Id = 8, Name = "أخصائي جراحة عظام", NormalizedName = "Orthopedic Surgeon", IconName = "orthopedics" },
    new Specialization { Id = 9, Name = "أخصائي نسائية وتوليد", NormalizedName = "Obstetrician-Gynecologist (OB-GYN)", IconName = "gynecology" },
    new Specialization { Id = 10, Name = "أخصائي أطفال", NormalizedName = "Pediatrician", IconName = "pediatrics" },
    new Specialization { Id = 11, Name = "أخصائي أورام", NormalizedName = "Oncologist", IconName = "oncology" },
    new Specialization { Id = 12, Name = "أخصائي كلى", NormalizedName = "Nephrologist", IconName = "nephrology" },
    new Specialization { Id = 13, Name = "أخصائي جهاز هضمي", NormalizedName = "Gastroenterologist", IconName = "gastroenterology" },
    new Specialization { Id = 14, Name = "أخصائي غدد صماء", NormalizedName = "Endocrinologist", IconName = "endocrinology" },
    new Specialization { Id = 15, Name = "أخصائي جراحة تجميل", NormalizedName = "Plastic Surgeon", IconName = "plastic-surgery" },
    new Specialization { Id = 16, Name = "أخصائي جراحة دماغ وأعصاب", NormalizedName = "Neurosurgeon", IconName = "neurosurgery" },
    new Specialization { Id = 17, Name = "أخصائي تخدير", NormalizedName = "Anesthesiologist", IconName = "anesthesiology" },
    new Specialization { Id = 18, Name = "أخصائي طب الأسرة", NormalizedName = "Family Medicine Specialist", IconName = "family-medicine" },
    new Specialization { Id = 19, Name = "أخصائي الطب النفسي", NormalizedName = "Psychiatrist", IconName = "psychiatry" },
    new Specialization { Id = 20, Name = "أخصائي أمراض معدية", NormalizedName = "Infectious Disease Specialist", IconName = "infectious-disease" },
    new Specialization { Id = 21, Name = "أخصائي أشعة", NormalizedName = "Radiologist", IconName = "radiology" },
    new Specialization { Id = 22, Name = "أخصائي طب طوارئ", NormalizedName = "Emergency Medicine Specialist", IconName = "emergency" },
    new Specialization { Id = 23, Name = "أخصائي روماتيزم", NormalizedName = "Rheumatologist", IconName = "rheumatology" },
    new Specialization { Id = 24, Name = "أخصائي صدرية", NormalizedName = "Pulmonologist", IconName = "pulmonology" },
    new Specialization { Id = 25, Name = "أخصائي طب مهني", NormalizedName = "Occupational Medicine Specialist", IconName = "occupational-medicine" },
    new Specialization { Id = 26, Name = "أخصائي طب رياضي", NormalizedName = "Sports Medicine Specialist", IconName = "sports-medicine" },
    new Specialization { Id = 27, Name = "أخصائي أمراض الدم", NormalizedName = "Hematologist", IconName = "hematology" },
    new Specialization { Id = 28, Name = "أخصائي علاج طبيعي", NormalizedName = "Physiotherapist", IconName = "physiotherapy" },
    new Specialization { Id = 29, Name = "أخصائي تغذية", NormalizedName = "Nutritionist", IconName = "nutrition" },
    new Specialization { Id = 30, Name = "أخصائي نطق وتخاطب", NormalizedName = "Speech Therapist", IconName = "speech-therapy" });
            });

            // Doctor
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => new { e.IsPubliclyVisible, e.SpecializationId });
                entity.HasIndex(e => e.NormalizedName);

                entity.HasOne(d => d.Specialization)
                    .WithMany()
                    .HasForeignKey(d => d.SpecializationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.User)
                    .WithOne()
                    .HasForeignKey<Doctor>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(d => d.Clinics)
                    .WithOne(c => c.Doctor)
                    .HasForeignKey(c => c.DoctorId);

                entity.HasMany(d => d.Reviews)
                    .WithOne(r => r.Doctor)
                    .HasForeignKey(r => r.DoctorId);

                entity.HasMany(d => d.DoctorSubscriptions)
                    .WithOne(ds => ds.Doctor)
                    .HasForeignKey(ds => ds.DoctorId);

                entity.HasMany(d => d.ReceivedMessages)
                    .WithOne(m => m.Receiver)
                    .HasForeignKey(m => m.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(d => d.Notifications)
                    .WithOne(n => n.Doctor)
                    .HasForeignKey(n => n.DoctorId);
            });

            // Clinic
            modelBuilder.Entity<Clinic>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Latitude).HasPrecision(9, 6);
                entity.Property(e => e.Longitude).HasPrecision(9, 6);
                entity.Property(e => e.ConsultationPrice).HasPrecision(10, 2);
                entity.HasIndex(e => new { e.IraqiProvince, e.IsVisible, e.IsDeleted });

                entity.HasOne(c => c.Doctor)
                    .WithMany(d => d.Clinics)
                    .HasForeignKey(c => c.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ClinicException
            modelBuilder.Entity<ClinicException>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClosureReason).HasMaxLength(500);
                entity.HasIndex(e => new { e.ClinicId, e.ExceptionDate })
                    .IsUnique()
                    .HasFilter("\"IsDeleted\" = false");

                entity.HasOne(e => e.Clinic)
                    .WithMany(c => c.Exceptions)
                    .HasForeignKey(e => e.ClinicId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SubscriptionPackage
            modelBuilder.Entity<SubscriptionPackage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasData(new SubscriptionPackage
                {
                    Id = 1,
                    Name = "أساسي",
                    NormalizedName = "Basic",
                    MaxClinics = 1,
                    MaxWeeklyDays = 4,
                    MaxDailyAppointments = 15,
                    ShowReviews = false,
                    ShowMessages = false,
                    MakeOffers = false,
                    MaxActiveOffers = 0,
                    EBooking = false,
                    EPayments = false,
                    Price = 0,
                    YearlyPrice = 0,

                }, new SubscriptionPackage
                {
                    Id = 2,
                    Name = "ذهبي",
                    NormalizedName = "Gold",
                    MaxClinics = 2,
                    MaxWeeklyDays = 5,
                    MaxDailyAppointments = 25,
                    ShowReviews = true,
                    ShowMessages = false,
                    MakeOffers = false,
                    MaxActiveOffers = 0,
                    EBooking = false,
                    EPayments = false,
                    Price = 25,
                    YearlyPrice = 250,

                }, new SubscriptionPackage
                {
                    Id = 3,
                    Name = "ألماس",
                    NormalizedName = "Diamond ",
                    MaxClinics = 3,
                    MaxWeeklyDays = 6,
                    MaxDailyAppointments = 35,
                    ShowReviews = true,
                    ShowMessages = true,
                    MakeOffers = true,
                    MaxActiveOffers = 1,
                    EBooking = true,
                    EPayments = true,
                    Price = 35,
                    YearlyPrice = 350

                }, new SubscriptionPackage
                {
                    Id = 4,
                    Name = "فاخر",
                    NormalizedName = "Premium",
                    MaxClinics = 5,
                    MaxWeeklyDays = 7,
                    MaxDailyAppointments = 1000,
                    ShowReviews = true,
                    ShowMessages = true,
                    MakeOffers = true,
                    MaxActiveOffers = 2,
                    EBooking = true,
                    EPayments = true,
                    Price = 45,
                    YearlyPrice = 450
                });
            });

            // DoctorSubscription
            modelBuilder.Entity<DoctorSubscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.DoctorId, e.Status, e.EndDate });

                entity.HasOne(ds => ds.Doctor)
                    .WithMany(d => d.DoctorSubscriptions)
                    .HasForeignKey(ds => ds.DoctorId);

                entity.HasOne(ds => ds.Package)
                    .WithMany()
                    .HasForeignKey(ds => ds.PackageId);
            });

            // Appointment
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(a => a.User)
                    .WithMany()
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Doctor)
                    .WithMany()
                    .HasForeignKey(a => a.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Clinic)
                    .WithMany(c => c.Appointments)
                    .HasForeignKey(a => a.ClinicId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(a => new { a.ClinicId, a.AppointmentDate, a.QueueNumber })
                    .IsUnique();
                entity.HasIndex(a => new { a.ClinicId, a.AppointmentDate, a.Status });
                entity.HasIndex(a => new { a.UserId, a.AppointmentDate, a.Status });
                entity.HasIndex(a => new { a.GuestPhoneNumber, a.AppointmentDate, a.Status });
            });

            // BookingOtpRequest
            modelBuilder.Entity<BookingOtpRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(30);
                entity.Property(e => e.CodeHash).IsRequired().HasMaxLength(128);
                entity.Property(e => e.CodeSalt).IsRequired().HasMaxLength(64);

                entity.HasOne(e => e.Appointment)
                    .WithMany()
                    .HasForeignKey(e => e.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.AppointmentId, e.IsUsed });
            });

            // Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(p => p.Appointment)
                    .WithMany()
                    .HasForeignKey(p => p.AppointmentId);
            });

            // RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(128);
                entity.Property(e => e.ReplacedByTokenHash).HasMaxLength(128);
                entity.HasIndex(e => e.TokenHash).IsUnique();
                entity.HasIndex(e => new { e.UserId, e.ExpiresAt, e.RevokedAt });

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // DeviceToken
            modelBuilder.Entity<DeviceToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(2048);
                entity.Property(e => e.Platform).IsRequired().HasMaxLength(30);
                entity.Property(e => e.DeviceId).HasMaxLength(200);
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => new { e.UserId, e.Platform, e.IsDeleted });

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UserPhoneOtpRequest
            modelBuilder.Entity<UserPhoneOtpRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(30);
                entity.Property(e => e.CodeHash).IsRequired().HasMaxLength(128);
                entity.Property(e => e.CodeSalt).IsRequired().HasMaxLength(64);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.IsUsed });
                entity.HasIndex(e => new { e.PhoneNumber, e.IsUsed });
            });

            // Review
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Comment).IsRequired().HasMaxLength(1000);
                entity.HasIndex(e => e.AppoinmentId)
                    .IsUnique()
                    .HasFilter("\"AppoinmentId\" IS NOT NULL AND \"IsDeleted\" = false");
                entity.ToTable(table => table.HasCheckConstraint(
                    "CK_Reviews_Rating_Range",
                    "\"Rating\" >= 1 AND \"Rating\" <= 5"));

                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Doctor)
                    .WithMany(d => d.Reviews)
                    .HasForeignKey(r => r.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Appointment)
                .WithMany(a => a.Reviews)
                .HasForeignKey(r=>r.AppoinmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            });

            // Message
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(m => m.Sender)
                    .WithMany()
                    .HasForeignKey(m => m.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Receiver)
                    .WithMany(d => d.ReceivedMessages)
                    .HasForeignKey(m => m.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(n => n.User)
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.Doctor)
                    .WithMany(d => d.Notifications)
                    .HasForeignKey(n => n.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Referral
            modelBuilder.Entity<Referral>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(r => r.InviterDoctor)
                    .WithMany()
                    .HasForeignKey(r => r.InviterDoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.InvitedDoctor)
                    .WithMany()
                    .HasForeignKey(r => r.InvitedDoctorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Day
            modelBuilder.Entity<Day>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.HasData(new Day
                {
                    Id = 1,
                    Name = "السبت",
                    NormalizedName = "Saturday"
                }, new Day
                {
                    Id = 2,
                    Name = "الاحد",
                    NormalizedName = "Sunday"
                },
                new Day
                {
                    Id = 3,
                    Name = "الاثنين",
                    NormalizedName = "Monday"
                },
                new Day
                {
                    Id = 4,
                    Name = "الثلاثاء",
                    NormalizedName = "Tuesday"
                },
                new Day
                {
                    Id = 5,
                    Name = "الاربعاء",
                    NormalizedName = "Wednesday"
                },
                new Day
                {
                    Id = 6,
                    Name = "الخميس",
                    NormalizedName = "Thursday"
                },
                new Day
                {
                    Id = 7,
                    Name = "الجمعة",
                    NormalizedName = "Friday"
                });
            });

            // DoctorAvailability
            modelBuilder.Entity<DoctorAvailability>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(da => da.Clinic)
                    .WithMany(c => c.Availabilities)
                    .HasForeignKey(da => da.ClinicId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(da => da.Day)
                    .WithMany()
                    .HasForeignKey(da => da.DayId);
            });

            //Feature
            modelBuilder.Entity<Feature>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

                entity.HasData(new Feature
                {
                    Id = 1,
                    Name = "تفعيل التقييم والردود",
                    NormalizedName = "ShowReviews",
                    Description = "",
                    IsPremiumOnly = true,
                }, new Feature
                {
                    Id = 2,
                    Name = "تفعيل زر الرسائل",
                    NormalizedName = "ShowMessages",
                    Description = "",
                    IsPremiumOnly = true,
                },
                new Feature
                {
                    Id = 3,
                    Name = "تفعيل الحجز الالكتروني",
                    NormalizedName = "EBooking",
                    Description = "",
                    IsPremiumOnly = true,
                },
                new Feature
                {
                    Id =4,
                    Name = "تفعيل الدفع الالكتروني",
                    NormalizedName = "EPayments",
                    Description = "",
                    IsPremiumOnly = true,
                });
            });


            //DoctorFeature
            modelBuilder.Entity<DoctorFeature>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(df => df.Doctor)
                .WithMany(d => d.DoctorFeatures)
                .HasForeignKey(df => df.DoctorId);

                entity.HasOne(df => df.Feature)
                .WithMany(d => d.DoctorFeatures)
                .HasForeignKey(df => df.FeatureId);
            });
        }
    }
}
