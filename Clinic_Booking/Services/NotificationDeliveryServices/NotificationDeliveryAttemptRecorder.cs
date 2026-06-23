using System.Text.Json;
using Clinic_Booking.Data;
using Clinic_Booking.Entities.NotificationDeliveryAttempt;

namespace Clinic_Booking.Services.NotificationDeliveryServices
{
    public static class NotificationDeliveryAttemptRecorder
    {
        public static void AddPushAttempt(
            ApplicationDbContext context,
            bool sent,
            Guid recipientUserId,
            string title,
            string body,
            IDictionary<string, string>? data,
            int? doctorId = null,
            int? clinicId = null,
            int? appointmentId = null,
            string? error = null)
        {
            var now = BusinessClock.Now();
            context.NotificationDeliveryAttempts.Add(new NotificationDeliveryAttempt
            {
                Channel = "Push",
                Status = sent ? "Succeeded" : "Failed",
                RecipientUserId = recipientUserId,
                Title = title,
                Body = body,
                PayloadJson = data == null ? null : JsonSerializer.Serialize(data),
                AttemptCount = 1,
                LastAttemptAt = now,
                NextAttemptAt = sent ? null : now.AddMinutes(15),
                LastError = sent ? null : error ?? "Push provider returned failure.",
                DoctorId = doctorId,
                ClinicId = clinicId,
                AppointmentId = appointmentId
            });
        }

        public static void AddWhatsAppAttempt(
            ApplicationDbContext context,
            bool sent,
            string recipientAddress,
            string title,
            string body,
            int? doctorId = null,
            int? clinicId = null,
            int? appointmentId = null,
            string? error = null,
            bool retryOnFailure = true)
        {
            var now = BusinessClock.Now();
            context.NotificationDeliveryAttempts.Add(new NotificationDeliveryAttempt
            {
                Channel = "WhatsApp",
                Status = sent ? "Succeeded" : "Failed",
                RecipientAddress = recipientAddress,
                Title = title,
                Body = body,
                AttemptCount = 1,
                LastAttemptAt = now,
                NextAttemptAt = sent || !retryOnFailure ? null : now.AddMinutes(15),
                LastError = sent ? null : error ?? "WhatsApp provider returned failure.",
                DoctorId = doctorId,
                ClinicId = clinicId,
                AppointmentId = appointmentId
            });
        }
    }
}
