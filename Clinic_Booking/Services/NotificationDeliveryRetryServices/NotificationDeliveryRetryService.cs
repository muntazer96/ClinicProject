using System.Text.Json;
using Clinic_Booking.Data;
using Clinic_Booking.Entities.NotificationDeliveryAttempt;
using Clinic_Booking.IServices.IPushNotificationServices;
using Clinic_Booking.IServices.IWhatsAppMessageServices;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.NotificationDeliveryRetryServices
{
    public class NotificationDeliveryRetryService : BackgroundService
    {
        private const int MaxAttempts = 5;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificationDeliveryRetryService> _logger;

        public NotificationDeliveryRetryService(
            IServiceScopeFactory scopeFactory,
            ILogger<NotificationDeliveryRetryService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task RunOnceAsync(CancellationToken cancellationToken = default)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var push = scope.ServiceProvider.GetRequiredService<IPushNotificationServices>();
            var whatsApp = scope.ServiceProvider.GetRequiredService<IWhatsAppMessageServices>();

            var now = DateTime.UtcNow;
            var attempts = await context.NotificationDeliveryAttempts
                .Where(attempt =>
                    attempt.Status == "Failed" &&
                    attempt.AttemptCount < MaxAttempts &&
                    (!attempt.NextAttemptAt.HasValue || attempt.NextAttemptAt <= now) &&
                    !attempt.IsDeleted)
                .OrderBy(attempt => attempt.NextAttemptAt)
                .Take(50)
                .ToListAsync(cancellationToken);

            foreach (var attempt in attempts)
            {
                await RetryAsync(push, whatsApp, attempt, cancellationToken);
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunOnceAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Notification delivery retry run failed.");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private static async Task RetryAsync(
            IPushNotificationServices push,
            IWhatsAppMessageServices whatsApp,
            NotificationDeliveryAttempt attempt,
            CancellationToken cancellationToken)
        {
            var sent = false;
            var error = string.Empty;

            if (attempt.Channel == "Push" && attempt.RecipientUserId.HasValue)
            {
                var data = string.IsNullOrWhiteSpace(attempt.PayloadJson)
                    ? null
                    : JsonSerializer.Deserialize<Dictionary<string, string>>(attempt.PayloadJson);
                sent = await push.SendToUserAsync(
                    attempt.RecipientUserId.Value,
                    attempt.Title,
                    attempt.Body,
                    data,
                    cancellationToken);
                error = "Push provider returned failure.";
            }
            else if (attempt.Channel == "WhatsApp" && !string.IsNullOrWhiteSpace(attempt.RecipientAddress))
            {
                sent = await whatsApp.SendMessageAsync(
                    attempt.RecipientAddress,
                    attempt.Body,
                    cancellationToken);
                error = "WhatsApp provider returned failure.";
            }
            else
            {
                error = "Retry skipped because recipient or channel is invalid.";
            }

            var now = DateTime.UtcNow;
            attempt.AttemptCount++;
            attempt.LastAttemptAt = now;
            attempt.Status = sent ? "Succeeded" : "Failed";
            attempt.LastError = sent ? null : error;
            attempt.NextAttemptAt = sent || attempt.AttemptCount >= MaxAttempts
                ? null
                : now.AddMinutes(15 * attempt.AttemptCount);
        }
    }
}
