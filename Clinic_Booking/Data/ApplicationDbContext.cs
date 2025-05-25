using Clinic_Booking.Entities.Appointment;
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
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<SubscriptionPackage> SubscriptionPackages { get; set; }
        public DbSet<DoctorSubscription> DoctorSubscriptions { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
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
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
            });

            // Specialization
            modelBuilder.Entity<Specialization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            });

            // Doctor
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

                entity.HasOne(d => d.Specialization)
                    .WithMany()
                    .HasForeignKey(d => d.SpecializationId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(d => d.Availabilities)
                    .WithOne(a => a.Doctor)
                    .HasForeignKey(a => a.DoctorId);

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

            // SubscriptionPackage
            modelBuilder.Entity<SubscriptionPackage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // DoctorSubscription
            modelBuilder.Entity<DoctorSubscription>(entity =>
            {
                entity.HasKey(e => e.Id);

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

                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Doctor)
                    .WithMany(d => d.Reviews)
                    .HasForeignKey(r => r.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);
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
            });

            // DoctorAvailability
            modelBuilder.Entity<DoctorAvailability>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(da => da.Doctor)
                    .WithMany(d => d.Availabilities)
                    .HasForeignKey(da => da.DoctorId);

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
