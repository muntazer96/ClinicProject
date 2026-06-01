using Clinic_Booking.Entities.Appointment;
using Clinic_Booking.Entities.Clinic;
using Clinic_Booking.Entities.ClinicException;
using Clinic_Booking.Entities.BookingOtpRequest;
using Clinic_Booking.Entities.Day;
using Clinic_Booking.Entities.Doctor;
using Clinic_Booking.Entities.DoctorAvailability;
using Clinic_Booking.Entities.DoctorFeature;
using Clinic_Booking.Entities.DoctorSubscription;
using Clinic_Booking.Entities.Feature;
using Clinic_Booking.Entities.Message;
using Clinic_Booking.Entities.Notification;
using Clinic_Booking.Entities.Payment;
using Clinic_Booking.Entities.Referral;
using Clinic_Booking.Entities.Review;
using Clinic_Booking.Entities.Role;
using Clinic_Booking.Entities.Specialization;
using Clinic_Booking.Entities.SubscriptionPackage;
using Clinic_Booking.Entities.User;
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
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Referral> Referrals { get; set; }
        public DbSet<Day> Days { get; set; }
        public DbSet<DoctorAvailability> DoctorAvailabilities { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<DoctorFeature> DoctorFeature { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var hasher = new PasswordHasher<AspNetUsers>();

            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);


                entity.HasData(new Entities.User.AspNetUsers()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow,
                    NormalizedUserName = "SUPARADMIN",
                    PasswordHash = hasher.HashPassword(null, "password"),
                    SecurityStamp = Guid.NewGuid().ToString(),
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
                    Id = Guid.NewGuid(),
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN",
                }, new AspNetRoles()
                {
                    Id = Guid.NewGuid(),
                    Name = "NormalUser",
                    NormalizedName = "NORMALUSER",
                }, new AspNetRoles()
                {
                    Id = Guid.NewGuid(),
                    Name = "DoctorUser",
                    NormalizedName = "DOCTORUSER",
                }, new AspNetRoles()
                {
                    Id = Guid.Parse("6f03ef0f-c1ac-43f6-9df2-2be6d5385b72"),
                    Name = AppRoles.ClinicStaff,
                    NormalizedName = "CLINICSTAFF",
                });
            });

            // Specialization
            modelBuilder.Entity<Specialization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.HasData(
    new Specialization { Id = 1, Name = "أخصائي باطنية", NormalizedName = "Internal Medicine Specialist" },
    new Specialization { Id = 2, Name = "أخصائي أنف وأذن وحنجرة", NormalizedName = "ENT Specialist (Ear, Nose, and Throat)" },
    new Specialization { Id = 3, Name = "أخصائي قلب", NormalizedName = "Cardiologist" },
    new Specialization { Id = 4, Name = "أخصائي عيون", NormalizedName = "Ophthalmologist" },
    new Specialization { Id = 5, Name = "أخصائي جلدية", NormalizedName = "Dermatologist" },
    new Specialization { Id = 6, Name = "أخصائي أعصاب", NormalizedName = "Neurologist" },
    new Specialization { Id = 7, Name = "أخصائي جراحة عامة", NormalizedName = "General Surgeon" },
    new Specialization { Id = 8, Name = "أخصائي جراحة عظام", NormalizedName = "Orthopedic Surgeon" },
    new Specialization { Id = 9, Name = "أخصائي نسائية وتوليد", NormalizedName = "Obstetrician-Gynecologist (OB-GYN)" },
    new Specialization { Id = 10, Name = "أخصائي أطفال", NormalizedName = "Pediatrician" },
    new Specialization { Id = 11, Name = "أخصائي أورام", NormalizedName = "Oncologist" },
    new Specialization { Id = 12, Name = "أخصائي كلى", NormalizedName = "Nephrologist" },
    new Specialization { Id = 13, Name = "أخصائي جهاز هضمي", NormalizedName = "Gastroenterologist" },
    new Specialization { Id = 14, Name = "أخصائي غدد صماء", NormalizedName = "Endocrinologist" },
    new Specialization { Id = 15, Name = "أخصائي جراحة تجميل", NormalizedName = "Plastic Surgeon" },
    new Specialization { Id = 16, Name = "أخصائي جراحة دماغ وأعصاب", NormalizedName = "Neurosurgeon" },
    new Specialization { Id = 17, Name = "أخصائي تخدير", NormalizedName = "Anesthesiologist" },
    new Specialization { Id = 18, Name = "أخصائي طب الأسرة", NormalizedName = "Family Medicine Specialist" },
    new Specialization { Id = 19, Name = "أخصائي الطب النفسي", NormalizedName = "Psychiatrist" },
    new Specialization { Id = 20, Name = "أخصائي أمراض معدية", NormalizedName = "Infectious Disease Specialist" },
    new Specialization { Id = 21, Name = "أخصائي أشعة", NormalizedName = "Radiologist" },
    new Specialization { Id = 22, Name = "أخصائي طب طوارئ", NormalizedName = "Emergency Medicine Specialist" },
    new Specialization { Id = 23, Name = "أخصائي روماتيزم", NormalizedName = "Rheumatologist" },
    new Specialization { Id = 24, Name = "أخصائي صدرية", NormalizedName = "Pulmonologist" },
    new Specialization { Id = 25, Name = "أخصائي طب مهني", NormalizedName = "Occupational Medicine Specialist" },
    new Specialization { Id = 26, Name = "أخصائي طب رياضي", NormalizedName = "Sports Medicine Specialist" },
    new Specialization { Id = 27, Name = "أخصائي أمراض الدم", NormalizedName = "Hematologist" },
    new Specialization { Id = 28, Name = "أخصائي علاج طبيعي", NormalizedName = "Physiotherapist" },
    new Specialization { Id = 29, Name = "أخصائي تغذية", NormalizedName = "Nutritionist" },
    new Specialization { Id = 30, Name = "أخصائي نطق وتخاطب", NormalizedName = "Speech Therapist" });
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
                .WithMany()
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
