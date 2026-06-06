using Clinic_Booking.IServices.IBookingSmsServices;
using Clinic_Booking.IServices.IWhatsAppMessageServices;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

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
            var message = $"مرحباً،\n\nرمز تأكيد الحجز الخاص بك هو:\n\n🔢 {code}\n\nيرجى إدخال هذا الرمز لإكمال تأكيد الموعد.\n\n⏳ الرمز صالح لمدة 10 دقائق.\n\nشكراً لاستخدامك تطبيق عياداتي.\n";
            var sent = await _whatsAppMessageServices.SendMessageAsync(phoneNumber, message);
            if (sent)
            {
                return;
            }

            _logger.LogInformation("Booking OTP for {PhoneNumber}: {Code}", phoneNumber, code);
        }
    }
}
