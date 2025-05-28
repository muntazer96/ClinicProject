namespace Clinic_Booking.IServices.IEmailServices
{
    public interface IEmailServices
    {
        Task SendAsync(string toEmail, string subject, string body);
    }
}
