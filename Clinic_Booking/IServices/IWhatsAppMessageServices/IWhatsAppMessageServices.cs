namespace Clinic_Booking.IServices.IWhatsAppMessageServices
{
    public interface IWhatsAppMessageServices
    {
        Task<bool> SendMessageAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
    }
}
