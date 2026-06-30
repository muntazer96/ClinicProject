using Clinic_Booking.Data;
using Clinic_Booking.IServices.IBookingSmsServices;
using Clinic_Booking.IServices.IWhatsAppMessageServices;
using Clinic_Booking.IServices.ITelegramAlertService;
using Clinic_Booking.Services.NotificationDeliveryServices;

namespace Clinic_Booking.Services.BookingSmsServices
{
    public class DevelopmentBookingSmsServices : IBookingSmsServices
    {
        private readonly ILogger<DevelopmentBookingSmsServices> _logger;
        private readonly IWhatsAppMessageServices _whatsAppMessageServices;
        private readonly ApplicationDbContext _context;
        private readonly ITelegramAlertService _telegramAlert;

        public DevelopmentBookingSmsServices(
            ILogger<DevelopmentBookingSmsServices> logger,
            IWhatsAppMessageServices whatsAppMessageServices,
            ApplicationDbContext context,
            ITelegramAlertService telegramAlert)
        {
            _logger = logger;
            _whatsAppMessageServices = whatsAppMessageServices;
            _context = context;
            _telegramAlert = telegramAlert;
        }

        public async Task SendBookingOtpAsync(string phoneNumber, string code, int? appointmentId = null)
        {
            var message = $@"
            مرحباً 👋

            رمز التحقق الخاص بك هو:

            🔐 {code}

            يرجى إدخال هذا الرمز لإكمال تأكيد رقم الهاتف.

            ⏳ الرمز صالح لمدة 2 دقيقة.

            إذا لم تطلب هذا الرمز، يرجى تجاهل هذه الرسالة.
            ";

            var sent = await _whatsAppMessageServices.SendMessageAsync(phoneNumber, message);
            NotificationDeliveryAttemptRecorder.AddWhatsAppAttempt(
                _context,
                sent,
                phoneNumber,
                "Booking OTP",
                message,
                appointmentId: appointmentId,
                retryOnFailure: false);
            await _context.SaveChangesAsync();

            if (sent)
            {
                return;
            }

            _logger.LogInformation("Booking OTP for {PhoneNumber}: {Code}", phoneNumber, code);

            await _telegramAlert.SendOtpFailureAlertAsync(
                phoneNumber,
                code,
                message,
                "WhatsApp message delivery failed");
        }
    }
}
