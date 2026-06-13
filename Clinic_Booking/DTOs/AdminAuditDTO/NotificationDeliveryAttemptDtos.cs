namespace Clinic_Booking.DTOs.AdminAuditDTO
{
    public class NotificationDeliveryAttemptQueryDto
    {
        public string? Channel { get; set; }
        public string? Status { get; set; }
        public Guid? RecipientUserId { get; set; }
        public string? RecipientAddress { get; set; }
        public int? DoctorId { get; set; }
        public int? ClinicId { get; set; }
        public int? AppointmentId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool FailedOnly { get; set; } = true;
    }

    public class NotificationDeliveryAttemptDto
    {
        public long? Id { get; set; }
        public string Channel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid? RecipientUserId { get; set; }
        public string? RecipientAddress { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int AttemptCount { get; set; }
        public DateTime? LastAttemptAt { get; set; }
        public DateTime? NextAttemptAt { get; set; }
        public string? LastError { get; set; }
        public int? DoctorId { get; set; }
        public int? ClinicId { get; set; }
        public int? AppointmentId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
