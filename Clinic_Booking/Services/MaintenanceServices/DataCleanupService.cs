using Clinic_Booking.Data;
using Clinic_Booking.Enums;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.MaintenanceServices
{
    public class DataCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DataCleanupService> _logger;

        public DataCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<DataCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task RunOnceAsync(CancellationToken cancellationToken = default)
        {
            return CleanupAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await RunOnceAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        private async Task CleanupAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTime.UtcNow;
            var expiredOtpCutoff = now.AddDays(-2);
            var readNotificationCutoff = now.AddDays(-30);

            var bookingOtpQuery = context.BookingOtpRequests
                .Where(request => request.ExpiresAt < expiredOtpCutoff);
            var phoneOtpQuery = context.UserPhoneOtpRequests
                .Where(request => request.ExpiresAt < expiredOtpCutoff);
            var notificationQuery = context.Notifications
                .Where(notification =>
                    notification.Status == NotificationStatus.Read &&
                    notification.ReadAt != null &&
                    notification.ReadAt < readNotificationCutoff);

            int bookingOtpDeleted;
            int phoneOtpDeleted;
            int notificationsDeleted;
            if (context.Database.IsRelational())
            {
                bookingOtpDeleted = await bookingOtpQuery.ExecuteDeleteAsync(cancellationToken);
                phoneOtpDeleted = await phoneOtpQuery.ExecuteDeleteAsync(cancellationToken);
                notificationsDeleted = await notificationQuery.ExecuteDeleteAsync(cancellationToken);
            }
            else
            {
                var bookingOtpRequests = await bookingOtpQuery.ToListAsync(cancellationToken);
                var phoneOtpRequests = await phoneOtpQuery.ToListAsync(cancellationToken);
                var notifications = await notificationQuery.ToListAsync(cancellationToken);

                context.BookingOtpRequests.RemoveRange(bookingOtpRequests);
                context.UserPhoneOtpRequests.RemoveRange(phoneOtpRequests);
                context.Notifications.RemoveRange(notifications);
                await context.SaveChangesAsync(cancellationToken);

                bookingOtpDeleted = bookingOtpRequests.Count;
                phoneOtpDeleted = phoneOtpRequests.Count;
                notificationsDeleted = notifications.Count;
            }

            if (bookingOtpDeleted > 0 || phoneOtpDeleted > 0 || notificationsDeleted > 0)
            {
                _logger.LogInformation(
                    "Data cleanup completed. BookingOtp={BookingOtp}, PhoneOtp={PhoneOtp}, Notifications={Notifications}",
                    bookingOtpDeleted,
                    phoneOtpDeleted,
                    notificationsDeleted);
            }
        }
    }
}
