namespace Clinic_Booking.IServices.IPushNotificationServices
{
    public interface IPushNotificationServices
    {
        Task SendToUserAsync(Guid userId, string title, string body, IDictionary<string, string>? data = null, CancellationToken cancellationToken = default);
    }
}
