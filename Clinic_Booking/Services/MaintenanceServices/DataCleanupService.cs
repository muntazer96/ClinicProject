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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanupAsync(stoppingToken);
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

            var bookingOtpDeleted = await context.BookingOtpRequests
                .Where(request => request.ExpiresAt < expiredOtpCutoff)
                .ExecuteDeleteAsync(cancellationToken);

            var phoneOtpDeleted = await context.UserPhoneOtpRequests
                .Where(request => request.ExpiresAt < expiredOtpCutoff)
                .ExecuteDeleteAsync(cancellationToken);

            var notificationsDeleted = await context.Notifications
                .Where(notification =>
                    notification.Status == NotificationStatus.Read &&
                    notification.ReadAt != null &&
                    notification.ReadAt < readNotificationCutoff)
                .ExecuteDeleteAsync(cancellationToken);

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
