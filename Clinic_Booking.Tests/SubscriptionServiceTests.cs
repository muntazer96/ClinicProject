using Clinic_Booking.Data;
using Clinic_Booking.DTOs.DoctorSubscriptionDTO;
using Clinic_Booking.Entities.Doctor;
using Clinic_Booking.Entities.DoctorFeature;
using Clinic_Booking.Entities.DoctorSubscription;
using Clinic_Booking.Entities.SubscriptionPackage;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.IPushNotificationServices;
using Clinic_Booking.Services.DoctorSubscriptionServices;
using Clinic_Booking.Services.SubscriptionExpirationServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clinic_Booking.Tests;

public class SubscriptionServiceTests
{
    [Fact]
    public async Task CreateSubscription_RejectsUnsupportedInitialStatus()
    {
        await using var context = CreateContext();
        context.Doctors.Add(CreateDoctor());
        await context.SaveChangesAsync();

        var service = new DoctorSubscriptionServices(context, new StubLoadServices());
        var result = await service.CreateSubscriptionAsync(new DoctorSubscriptionAddDto
        {
            DoctorId = 1,
            PackageId = 1,
            Status = SubscriptionStatus.Expired
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateSubscription_RejectsDuplicateActiveSubscription()
    {
        await using var context = CreateContext();
        context.Doctors.Add(CreateDoctor());
        context.DoctorSubscriptions.Add(new DoctorSubscription
        {
            DoctorId = 1,
            PackageId = 1,
            StartDate = BusinessClock.Now().AddDays(-1),
            EndDate = BusinessClock.Now().AddDays(1),
            Status = SubscriptionStatus.Active
        });
        await context.SaveChangesAsync();

        var service = new DoctorSubscriptionServices(context, new StubLoadServices());
        var result = await service.CreateSubscriptionAsync(new DoctorSubscriptionAddDto
        {
            DoctorId = 1,
            PackageId = 1,
            Status = SubscriptionStatus.Active
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateSubscription_CreatesAuditLog()
    {
        await using var context = CreateContext();
        context.Doctors.Add(CreateDoctor());
        context.SubscriptionPackages.Add(CreatePackage());
        await context.SaveChangesAsync();

        var service = new DoctorSubscriptionServices(context, new StubLoadServices());
        var result = await service.CreateSubscriptionAsync(new DoctorSubscriptionAddDto
        {
            DoctorId = 1,
            PackageId = 1,
            Status = SubscriptionStatus.Active
        });

        Assert.IsType<OkObjectResult>(result);
        Assert.True(await context.AuditLogs.AnyAsync(log =>
            log.Action == "SubscriptionCreatedAndActivated" &&
            log.DoctorId == 1 &&
            log.EntityType == "DoctorSubscription"));
    }

    [Fact]
    public async Task ExpirationRun_ExpiresEndedSubscriptionAndFallsBackToBasicPlan()
    {
        var databaseName = Guid.NewGuid().ToString();
        var doctorUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var pushNotifications = new StubPushNotificationServices();
        var services = new ServiceCollection()
            .AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName))
            .AddSingleton<IPushNotificationServices>(pushNotifications)
            .BuildServiceProvider();

        await using (var scope = services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Doctors.Add(CreateDoctor(doctorUserId));
            context.SubscriptionPackages.AddRange(
                new SubscriptionPackage
                {
                    Id = 1,
                    Name = "Basic",
                    NormalizedName = "Basic",
                    MaxClinics = 1,
                    MaxDailyAppointments = 10,
                    MaxWeeklyDays = 3
                },
                new SubscriptionPackage
                {
                    Id = 2,
                    Name = "Gold",
                    NormalizedName = "Gold",
                    MaxClinics = 5,
                    MaxDailyAppointments = 30,
                    MaxWeeklyDays = 7
                });
            context.DoctorSubscriptions.Add(new DoctorSubscription
            {
                DoctorId = 1,
                PackageId = 2,
                StartDate = BusinessClock.Now().AddDays(-30),
                EndDate = BusinessClock.Now().AddMinutes(-5),
                Status = SubscriptionStatus.Active
            });
            context.DoctorFeature.Add(new DoctorFeature
            {
                DoctorId = 1,
                FeatureId = 1,
                IsEnabled = true
            });
            await context.SaveChangesAsync();
        }

        var service = new SubscriptionExpirationService(services.GetRequiredService<IServiceScopeFactory>());
        await service.RunOnceAsync();

        await using var verificationScope = services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(SubscriptionStatus.Expired, await verificationContext.DoctorSubscriptions
            .Where(subscription => subscription.PackageId == 2)
            .Select(subscription => subscription.Status)
            .SingleAsync());
        Assert.True(await verificationContext.DoctorSubscriptions.AnyAsync(subscription =>
            subscription.DoctorId == 1 &&
            subscription.PackageId == 1 &&
            subscription.Status == SubscriptionStatus.Active));
        Assert.False(await verificationContext.DoctorFeature
            .Where(feature => feature.DoctorId == 1 && feature.FeatureId == 1)
            .Select(feature => feature.IsEnabled)
            .SingleAsync());
        Assert.Equal(1, await verificationContext.Notifications.CountAsync(notification => notification.DoctorId == 1));
        Assert.Equal(1, pushNotifications.SentCount);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Doctor CreateDoctor()
    {
        return new Doctor
        {
            Id = 1,
            Name = "Doctor",
            NormalizedName = "DOCTOR",
            Description = "Test doctor",
            ImageName = "default.png"
        };
    }

    private static Doctor CreateDoctor(Guid userId)
    {
        var doctor = CreateDoctor();
        doctor.UserId = userId;
        return doctor;
    }

    private static SubscriptionPackage CreatePackage()
    {
        return new SubscriptionPackage
        {
            Id = 1,
            Name = "Basic",
            NormalizedName = "Basic",
            MaxClinics = 1,
            MaxDailyAppointments = 10,
            MaxWeeklyDays = 3
        };
    }

    private sealed class StubLoadServices : ILoadServices
    {
        public Guid? GetCurrentUserId() => Guid.Empty;
        public string SandEmailHTMLTemplate(string confirmationLink) => confirmationLink;
        public string ResetPasswordHTMLTemplate(string resetLink) => resetLink;
    }

    private sealed class StubPushNotificationServices : IPushNotificationServices
    {
        public int SentCount { get; private set; }

        public Task<bool> SendToUserAsync(
            Guid userId,
            string title,
            string body,
            IDictionary<string, string>? data = null,
            CancellationToken cancellationToken = default)
        {
            SentCount++;
            return Task.FromResult(true);
        }
    }
}
