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
            var messageCutoff = now.AddDays(-30);

            var bookingOtpQuery = context.BookingOtpRequests
                .Where(request => request.ExpiresAt < expiredOtpCutoff);
            var phoneOtpQuery = context.UserPhoneOtpRequests
                .Where(request => request.ExpiresAt < expiredOtpCutoff);
            var notificationQuery = context.Notifications
                .Where(notification =>
                    notification.Status == NotificationStatus.Read &&
                    notification.ReadAt != null &&
                    notification.ReadAt < readNotificationCutoff);
            var messageQuery = context.Messages
                .IgnoreQueryFilters()
                .Where(message => message.SentAt < messageCutoff);

            var messageImageNames = await messageQuery
                .Where(message => message.ImageName != null && message.ImageName != "")
                .Select(message => message.ImageName!)
                .ToListAsync(cancellationToken);

            int bookingOtpDeleted;
            int phoneOtpDeleted;
            int notificationsDeleted;
            int messagesDeleted;
            if (context.Database.IsRelational())
            {
                bookingOtpDeleted = await bookingOtpQuery.ExecuteDeleteAsync(cancellationToken);
                phoneOtpDeleted = await phoneOtpQuery.ExecuteDeleteAsync(cancellationToken);
                notificationsDeleted = await notificationQuery.ExecuteDeleteAsync(cancellationToken);
                messagesDeleted = await messageQuery.ExecuteDeleteAsync(cancellationToken);
            }
            else
            {
                var bookingOtpRequests = await bookingOtpQuery.ToListAsync(cancellationToken);
                var phoneOtpRequests = await phoneOtpQuery.ToListAsync(cancellationToken);
                var notifications = await notificationQuery.ToListAsync(cancellationToken);
                var messages = await messageQuery.ToListAsync(cancellationToken);

                context.BookingOtpRequests.RemoveRange(bookingOtpRequests);
                context.UserPhoneOtpRequests.RemoveRange(phoneOtpRequests);
                context.Notifications.RemoveRange(notifications);
                context.Messages.RemoveRange(messages);
                await context.SaveChangesAsync(cancellationToken);

                bookingOtpDeleted = bookingOtpRequests.Count;
                phoneOtpDeleted = phoneOtpRequests.Count;
                notificationsDeleted = notifications.Count;
                messagesDeleted = messages.Count;
            }

            var messageImagesDeleted = DeleteMessageImages(messageImageNames);

            if (bookingOtpDeleted > 0 || phoneOtpDeleted > 0 || notificationsDeleted > 0 || messagesDeleted > 0 || messageImagesDeleted > 0)
            {
                _logger.LogInformation(
                    "Data cleanup completed. BookingOtp={BookingOtp}, PhoneOtp={PhoneOtp}, Notifications={Notifications}, Messages={Messages}, MessageImages={MessageImages}",
                    bookingOtpDeleted,
                    phoneOtpDeleted,
                    notificationsDeleted,
                    messagesDeleted,
                    messageImagesDeleted);
            }
        }

        private int DeleteMessageImages(IEnumerable<string> imageNames)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MessageImages");
            var deleted = 0;

            foreach (var imageName in imageNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    var safeFileName = Path.GetFileName(imageName);
                    if (string.IsNullOrWhiteSpace(safeFileName)) continue;

                    var filePath = Path.Combine(folderPath, safeFileName);
                    if (!File.Exists(filePath)) continue;

                    File.Delete(filePath);
                    deleted++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete message image {ImageName}", imageName);
                }
            }

            return deleted;
        }
    }
}
