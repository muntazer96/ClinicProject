namespace Clinic_Booking.DTOs.AdminAuditDTO
{
    public class AuditLogQueryDto
    {
        public string? Action { get; set; }
        public string? EntityType { get; set; }
        public Guid? UserId { get; set; }
        public int? DoctorId { get; set; }
        public int? ClinicId { get; set; }
        public int? AppointmentId { get; set; }
        public int? SubscriptionId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public class AuditLogDto
    {
        public long? Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public Guid? UserId { get; set; }
        public int? DoctorId { get; set; }
        public int? ClinicId { get; set; }
        public int? AppointmentId { get; set; }
        public int? SubscriptionId { get; set; }
        public string? Details { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
