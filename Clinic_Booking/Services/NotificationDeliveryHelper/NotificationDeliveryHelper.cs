using System.Text.Json;
using Clinic_Booking.Data;
using Clinic_Booking.Entities.Notification;
using Clinic_Booking.Entities.NotificationDeliveryAttempt;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.INotificationDeliveryHelper;
using Clinic_Booking.IServices.IPushNotificationServices;
using Clinic_Booking.IServices.IWhatsAppMessageServices;
using Clinic_Booking.Services.NotificationDeliveryServices;

namespace Clinic_Booking.Services.NotificationDeliveryHelper
{
    public class NotificationDeliveryHelper : INotificationDeliveryHelper
    {
        private readonly ApplicationDbContext _context;
        private readonly IPushNotificationServices _pushNotificationServices;
        private readonly IWhatsAppMessageServices _whatsAppMessageServices;

        public NotificationDeliveryHelper(
            ApplicationDbContext context,
            IPushNotificationServices pushNotificationServices,
            IWhatsAppMessageServices whatsAppMessageServices)
        {
            _context = context;
            _pushNotificationServices = pushNotificationServices;
            _whatsAppMessageServices = whatsAppMessageServices;
        }

        public async Task SendPendingNotificationsAsync(List<PendingAppointmentNotification> notifications)
        {
            if (notifications.Count == 0)
            {
                return;
            }

            var now = DateTime.UtcNow;
            _context.Notifications.AddRange(notifications.Select(notification => new Notification
            {
                DoctorId = notification.DoctorId,
                Message = notification.Body,
                CreatedAt = now,
                Status = NotificationStatus.Unread
            }));
            await _context.SaveChangesAsync();

            foreach (var notification in notifications.Where(item => item.UserId.HasValue))
            {
                var data = new Dictionary<string, string>
                {
                    ["type"] = "booking",
                    ["appointmentId"] = notification.AppointmentId.ToString(),
                    ["doctorId"] = notification.DoctorId.ToString(),
                    ["clinicId"] = notification.ClinicId.ToString(),
                    ["status"] = notification.Status.ToString()
                };
                var sent = await _pushNotificationServices.SendToUserAsync(
                    notification.UserId!.Value,
                    notification.Title,
                    notification.Body,
                    data);
                NotificationDeliveryAttemptRecorder.AddPushAttempt(
                    _context,
                    sent,
                    notification.UserId.Value,
                    notification.Title,
                    notification.Body,
                    data,
                    doctorId: notification.DoctorId,
                    clinicId: notification.ClinicId,
                    appointmentId: notification.AppointmentId,
                    error: sent ? null : "Push provider returned failure.");
            }

            foreach (var notification in notifications.Where(item =>
                         !item.UserId.HasValue &&
                         !string.IsNullOrWhiteSpace(item.GuestPhoneNumber)))
            {
                var sent = await _whatsAppMessageServices.SendMessageAsync(
                    notification.GuestPhoneNumber!,
                    notification.Body);
                NotificationDeliveryAttemptRecorder.AddWhatsAppAttempt(
                    _context,
                    sent,
                    notification.GuestPhoneNumber!,
                    notification.Title,
                    notification.Body,
                    doctorId: notification.DoctorId,
                    clinicId: notification.ClinicId,
                    appointmentId: notification.AppointmentId,
                    error: sent ? null : "WhatsApp provider returned failure.");
            }

            await _context.SaveChangesAsync();
        }
    }
}
