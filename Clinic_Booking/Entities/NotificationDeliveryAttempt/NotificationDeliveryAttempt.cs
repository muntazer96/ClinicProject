using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.NotificationDeliveryAttempt
{
    public class NotificationDeliveryAttempt : BaseEntity<long>
    {
        public string Channel { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public Guid? RecipientUserId { get; set; }
        public string? RecipientAddress { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? PayloadJson { get; set; }
        public int AttemptCount { get; set; }
        public DateTime? LastAttemptAt { get; set; }
        public DateTime? NextAttemptAt { get; set; }
        public string? LastError { get; set; }
        public int? DoctorId { get; set; }
        public int? ClinicId { get; set; }
        public int? AppointmentId { get; set; }
    }
}
