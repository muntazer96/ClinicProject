using Clinic_Booking.Data;
using Clinic_Booking.Entities.BookingOtpRequest;
using Clinic_Booking.Entities.Notification;
using Clinic_Booking.Entities.UserPhoneOtpRequest;
using Clinic_Booking.Enums;
using Clinic_Booking.Services.MaintenanceServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Clinic_Booking.Tests;

public class DataCleanupServiceTests
{
    [Fact]
    public async Task RunOnce_RemovesOnlyExpiredOtpAndOldReadNotifications()
    {
        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection()
            .AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName))
            .BuildServiceProvider();
        var now = BusinessClock.Now();
        var userId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        await using (var scope = services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.BookingOtpRequests.AddRange(
                CreateBookingOtp(1, now.AddDays(-3)),
                CreateBookingOtp(2, now.AddDays(-1)));
            context.UserPhoneOtpRequests.AddRange(
                CreateUserPhoneOtp(userId, now.AddDays(-3)),
                CreateUserPhoneOtp(userId, now.AddDays(-1)));
            context.Notifications.AddRange(
                CreateNotification(NotificationStatus.Read, now.AddDays(-31)),
                CreateNotification(NotificationStatus.Read, now.AddDays(-29)),
                CreateNotification(NotificationStatus.Unread, now.AddDays(-365)));
            await context.SaveChangesAsync();
        }

        var cleanup = new DataCleanupService(
            services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<DataCleanupService>.Instance);
        await cleanup.RunOnceAsync();

        await using var verificationScope = services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal([2], await verificationContext.BookingOtpRequests
            .OrderBy(request => request.AppointmentId)
            .Select(request => request.AppointmentId)
            .ToListAsync());
        Assert.Single(await verificationContext.UserPhoneOtpRequests.ToListAsync());
        Assert.Equal(2, await verificationContext.Notifications.CountAsync());
        Assert.True(await verificationContext.Notifications.AnyAsync(notification =>
            notification.Status == NotificationStatus.Read &&
            notification.ReadAt > now.AddDays(-30)));
        Assert.True(await verificationContext.Notifications.AnyAsync(notification =>
            notification.Status == NotificationStatus.Unread &&
            notification.ReadAt < now.AddDays(-300)));
    }

    private static BookingOtpRequest CreateBookingOtp(int appointmentId, DateTime expiresAt)
    {
        return new BookingOtpRequest
        {
            AppointmentId = appointmentId,
            PhoneNumber = "07700000000",
            CodeHash = "hash",
            CodeSalt = "salt",
            ExpiresAt = expiresAt,
            SentAt = expiresAt.AddMinutes(-5)
        };
    }

    private static UserPhoneOtpRequest CreateUserPhoneOtp(Guid userId, DateTime expiresAt)
    {
        return new UserPhoneOtpRequest
        {
            UserId = userId,
            PhoneNumber = "07700000000",
            CodeHash = "hash",
            CodeSalt = "salt",
            ExpiresAt = expiresAt,
            SentAt = expiresAt.AddMinutes(-5)
        };
    }

    private static Notification CreateNotification(NotificationStatus status, DateTime readAt)
    {
        return new Notification
        {
            Message = "Notification",
            CreatedAt = readAt.AddDays(-1),
            Status = status,
            ReadAt = readAt
        };
    }
}
