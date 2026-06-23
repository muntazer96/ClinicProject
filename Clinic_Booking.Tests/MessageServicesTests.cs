using Clinic_Booking.Data;
using Clinic_Booking.Entities.Appointment;
using Clinic_Booking.Entities.Doctor;
using Clinic_Booking.Entities.DoctorFeature;
using Clinic_Booking.Entities.DoctorSubscription;
using Clinic_Booking.Entities.Feature;
using Clinic_Booking.Entities.SubscriptionPackage;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.IPushNotificationServices;
using Clinic_Booking.Services.MessageServices;
using Clinic_Booking.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Tests;

public class MessageServicesTests
{
    private static readonly Guid PatientUserId = Guid.Parse("22060f1a-95bf-4c8f-901d-19ea3eab0e68");
    private static readonly Guid DoctorUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task CanSendMessageAsync_UsesBaghdadClockForCompletedAppointmentWindow()
    {
        await using var context = CreateContext();
        SeedMessagingEnabledDoctor(context);
        context.Appointments.Add(new Appointment
        {
            Id = 1,
            UserId = PatientUserId,
            DoctorId = 1,
            ClinicId = 1,
            AppointmentDate = BusinessClock.Today(),
            Status = AppointmentStatus.Completed,
            Code = "A1"
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        var canSend = await service.CanSendMessageAsync(PatientUserId, DoctorUserId);

        Assert.True(canSend);
    }

    private static MessageServices CreateService(ApplicationDbContext context)
    {
        return new MessageServices(
            context,
            new StubLoadServices(PatientUserId),
            new StubPushNotificationServices(),
            hubContext: null!,
            new OnlineUserTracker());
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedMessagingEnabledDoctor(ApplicationDbContext context)
    {
        context.Doctors.Add(new Doctor
        {
            Id = 1,
            UserId = DoctorUserId,
            Name = "Doctor",
            NormalizedName = "DOCTOR",
            Description = "Test doctor",
            ImageName = "default.png"
        });
        context.SubscriptionPackages.Add(new SubscriptionPackage
        {
            Id = 1,
            Name = "Messaging package",
            NormalizedName = "MESSAGING",
            ShowMessages = true
        });
        context.DoctorSubscriptions.Add(new DoctorSubscription
        {
            Id = 1,
            DoctorId = 1,
            PackageId = 1,
            StartDate = BusinessClock.Now().AddDays(-1),
            EndDate = BusinessClock.Now().AddDays(30),
            Status = SubscriptionStatus.Active
        });
        context.Features.Add(new Feature
        {
            Id = 1,
            Name = "ShowMessages",
            NormalizedName = "ShowMessages"
        });
        context.DoctorFeature.Add(new DoctorFeature
        {
            Id = 1,
            DoctorId = 1,
            FeatureId = 1,
            IsEnabled = true
        });
    }

    private sealed class StubLoadServices(Guid userId) : ILoadServices
    {
        public Guid? GetCurrentUserId() => userId;
        public string SandEmailHTMLTemplate(string confirmationLink) => confirmationLink;
        public string ResetPasswordHTMLTemplate(string resetLink) => resetLink;
    }

    private sealed class StubPushNotificationServices : IPushNotificationServices
    {
        public Task<bool> SendToUserAsync(
            Guid userId,
            string title,
            string body,
            IDictionary<string, string>? data = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}
