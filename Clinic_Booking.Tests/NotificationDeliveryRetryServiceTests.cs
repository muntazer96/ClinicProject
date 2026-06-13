using Clinic_Booking.Data;
using Clinic_Booking.Entities.NotificationDeliveryAttempt;
using Clinic_Booking.IServices.IPushNotificationServices;
using Clinic_Booking.IServices.IWhatsAppMessageServices;
using Clinic_Booking.Services.NotificationDeliveryRetryServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Clinic_Booking.Tests;

public class NotificationDeliveryRetryServiceTests
{
    [Fact]
    public async Task RunOnce_RetriesFailedWhatsAppDeliveryAndMarksSucceeded()
    {
        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection()
            .AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName))
            .AddSingleton<IPushNotificationServices, StubPushNotificationServices>()
            .AddSingleton<IWhatsAppMessageServices, StubWhatsAppMessageServices>()
            .BuildServiceProvider();

        await using (var scope = services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.NotificationDeliveryAttempts.Add(new NotificationDeliveryAttempt
            {
                Channel = "WhatsApp",
                Status = "Failed",
                RecipientAddress = "07700000000",
                Title = "Test",
                Body = "Retry me",
                AttemptCount = 1,
                LastAttemptAt = DateTime.UtcNow.AddMinutes(-20),
                NextAttemptAt = DateTime.UtcNow.AddMinutes(-1)
            });
            await context.SaveChangesAsync();
        }

        var service = new NotificationDeliveryRetryService(
            services.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<NotificationDeliveryRetryService>.Instance);
        await service.RunOnceAsync();

        await using var verificationScope = services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var attempt = await verificationContext.NotificationDeliveryAttempts.SingleAsync();
        Assert.Equal("Succeeded", attempt.Status);
        Assert.Equal(2, attempt.AttemptCount);
        Assert.Null(attempt.NextAttemptAt);
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

    private sealed class StubWhatsAppMessageServices : IWhatsAppMessageServices
    {
        public Task<bool> SendMessageAsync(
            string phoneNumber,
            string message,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}
