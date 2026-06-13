using Clinic_Booking.Data;
using Clinic_Booking.IServices.IWhatsAppMessageServices;
using Clinic_Booking.Services.BookingSmsServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

public class NotificationDeliveryRecordingTests
{
    [Fact]
    public async Task BookingOtpWhatsAppFailure_IsRecordedWithoutRetry()
    {
        await using var context = CreateContext();
        var service = new DevelopmentBookingSmsServices(
            NullLogger<DevelopmentBookingSmsServices>.Instance,
            new StubWhatsAppMessageServices(false),
            context);

        await service.SendBookingOtpAsync("07700000000", "123456", appointmentId: 17);

        var attempt = await context.NotificationDeliveryAttempts.SingleAsync();
        Assert.Equal("WhatsApp", attempt.Channel);
        Assert.Equal("Failed", attempt.Status);
        Assert.Equal("07700000000", attempt.RecipientAddress);
        Assert.Equal(17, attempt.AppointmentId);
        Assert.Equal(1, attempt.AttemptCount);
        Assert.Null(attempt.NextAttemptAt);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private sealed class StubWhatsAppMessageServices(bool sendResult) : IWhatsAppMessageServices
    {
        public Task<bool> SendMessageAsync(
            string phoneNumber,
            string message,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(sendResult);
        }
    }
}
