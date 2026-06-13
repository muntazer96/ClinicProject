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
            var now = DateTime.UtcNow;
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
    }
}
