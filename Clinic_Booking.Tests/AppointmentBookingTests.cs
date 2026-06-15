using Clinic_Booking.Configuration;
using Clinic_Booking.Data;
using Clinic_Booking.DTOs.AppointmentDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.Clinic;
using Clinic_Booking.Entities.Day;
using Clinic_Booking.Entities.Doctor;
using Clinic_Booking.Entities.DoctorAvailability;
using Clinic_Booking.Entities.DoctorFeature;
using Clinic_Booking.Entities.DoctorSubscription;
using Clinic_Booking.Entities.Feature;
using Clinic_Booking.Entities.SubscriptionPackage;
using Clinic_Booking.Entities.User;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.IBookingSmsServices;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.IPushNotificationServices;
using Clinic_Booking.Services.AppointmentServices;
using Clinic_Booking.Services.MessageServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Clinic_Booking.Tests;

public class AppointmentBookingTests
{
    private static readonly Guid PatientUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid DoctorUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task CreateAppointment_ConfirmedUserPhone_DoesNotRequireBookingOtp()
    {
        await using var context = CreateContext();
        var appointmentDate = Next(DayOfWeek.Monday);
        SeedBookableClinic(context, appointmentDate.DayOfWeek);
        context.AspNetUsers.Add(new AspNetUsers
        {
            Id = PatientUserId,
            UserName = "patient",
            PhoneNumber = "07700000000",
            PhoneNumberConfirmed = true,
            EmailConfirmed = false
        });
        await context.SaveChangesAsync();

        var sms = new StubBookingSmsServices();
        var service = CreateService(context, sms, PatientUserId);

        var result = await service.CreateAppointmentAsync(new AddAppointmentDto
        {
            DoctorId = 1,
            ClinicId = 1,
            AppointmentDate = appointmentDate
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResponseDto<object>>(ok.Value);
        Assert.False((bool)response.Data!.GetType().GetProperty("RequiresOtp")!.GetValue(response.Data)!);
        Assert.True((bool)response.Data.GetType().GetProperty("IsPhoneConfirmed")!.GetValue(response.Data)!);
        Assert.Equal(0, sms.SentCount);
        Assert.Equal(0, await context.BookingOtpRequests.CountAsync());
    }

    private static AppointmentServices CreateService(
        ApplicationDbContext context,
        IBookingSmsServices sms,
        Guid currentUserId)
    {
        return new AppointmentServices(
            context,
            new StubLoadServices(currentUserId),
            sms,
            new StubPushNotificationServices(),
            hubContext: null!,
            new OnlineUserTracker(),
            Options.Create(new BookingOtpOptions { Enabled = true }),
            NullLogger<AppointmentServices>.Instance);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new ApplicationDbContext(options);
    }

    private static void SeedBookableClinic(ApplicationDbContext context, DayOfWeek dayOfWeek)
    {
        context.AspNetUsers.Add(new AspNetUsers
        {
            Id = DoctorUserId,
            UserName = "doctor",
            PhoneNumber = "07800000000",
            PhoneNumberConfirmed = true
        });
        context.Doctors.Add(new Doctor
        {
            Id = 1,
            UserId = DoctorUserId,
            Name = "Doctor",
            NormalizedName = "DOCTOR",
            Description = "Test doctor",
            ImageName = "default.png"
        });
        context.Clinics.Add(new Clinic
        {
            Id = 1,
            DoctorId = 1,
            Name = "Clinic",
            Address = "Address",
            BookingWindowDays = 31
        });
        context.Days.Add(new Day
        {
            Id = 1,
            Name = dayOfWeek.ToString(),
            NormalizedName = dayOfWeek.ToString()
        });
        context.DoctorAvailabilities.Add(new DoctorAvailability
        {
            Id = 1,
            ClinicId = 1,
            DayId = 1,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(17),
            MaxAppointments = 10,
            IsAvailable = true
        });
        context.SubscriptionPackages.Add(new SubscriptionPackage
        {
            Id = 1,
            Name = "Booking package",
            NormalizedName = "BOOKING",
            MaxClinics = 1,
            MaxDailyAppointments = 10,
            MaxWeeklyDays = 4,
            EBooking = true
        });
        context.DoctorSubscriptions.Add(new DoctorSubscription
        {
            DoctorId = 1,
            PackageId = 1,
            StartDate = BusinessClock.Now().AddDays(-1),
            EndDate = BusinessClock.Now().AddDays(30),
            Status = SubscriptionStatus.Active
        });
        context.Features.Add(new Feature
        {
            Id = 1,
            Name = "EBooking",
            NormalizedName = "EBooking"
        });
        context.DoctorFeature.Add(new DoctorFeature
        {
            DoctorId = 1,
            FeatureId = 1,
            IsEnabled = true
        });
    }

    private static DateTime Next(DayOfWeek dayOfWeek)
    {
        var date = DateTime.Today.AddDays(1);
        while (date.DayOfWeek != dayOfWeek)
        {
            date = date.AddDays(1);
        }

        return date.Date;
    }

    private sealed class StubLoadServices(Guid userId) : ILoadServices
    {
        public Guid? GetCurrentUserId() => userId;
        public string SandEmailHTMLTemplate(string confirmationLink) => confirmationLink;
        public string ResetPasswordHTMLTemplate(string resetLink) => resetLink;
    }

    private sealed class StubBookingSmsServices : IBookingSmsServices
    {
        public int SentCount { get; private set; }

        public Task SendBookingOtpAsync(string phoneNumber, string code, int? appointmentId = null)
        {
            SentCount++;
            return Task.CompletedTask;
        }
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
