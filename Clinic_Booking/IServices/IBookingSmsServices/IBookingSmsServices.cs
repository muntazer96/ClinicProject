namespace Clinic_Booking.IServices.IBookingSmsServices
{
    public interface IBookingSmsServices
    {
        Task SendBookingOtpAsync(string phoneNumber, string code, int? appointmentId = null);
    }
}
