using Clinic_Booking.IServices.IBookingSmsServices;

namespace Clinic_Booking.Services.BookingSmsServices
{
    public class DevelopmentBookingSmsServices : IBookingSmsServices
    {
        private readonly ILogger<DevelopmentBookingSmsServices> _logger;

        public DevelopmentBookingSmsServices(ILogger<DevelopmentBookingSmsServices> logger)
        {
            _logger = logger;
        }

        public Task SendBookingOtpAsync(string phoneNumber, string code)
        {
            _logger.LogInformation("Booking OTP for {PhoneNumber}: {Code}", phoneNumber, code);
            return Task.CompletedTask;
        }
    }
}
