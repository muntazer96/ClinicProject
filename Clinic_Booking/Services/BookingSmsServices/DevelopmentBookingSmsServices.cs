using Clinic_Booking.IServices.IBookingSmsServices;
using Clinic_Booking.IServices.IWhatsAppMessageServices;

namespace Clinic_Booking.Services.BookingSmsServices
{
    public class DevelopmentBookingSmsServices : IBookingSmsServices
    {
        private readonly ILogger<DevelopmentBookingSmsServices> _logger;
        private readonly IWhatsAppMessageServices _whatsAppMessageServices;

        public DevelopmentBookingSmsServices(
            ILogger<DevelopmentBookingSmsServices> logger,
            IWhatsAppMessageServices whatsAppMessageServices)
        {
            _logger = logger;
            _whatsAppMessageServices = whatsAppMessageServices;
        }

        public async Task SendBookingOtpAsync(string phoneNumber, string code)
        {
            var message = $"رمز التحقق الخاص بحجزك هو: {code}";
            var sent = await _whatsAppMessageServices.SendMessageAsync(phoneNumber, message);
            if (sent)
            {
                return;
            }

            _logger.LogInformation("Booking OTP for {PhoneNumber}: {Code}", phoneNumber, code);
        }
    }
}
