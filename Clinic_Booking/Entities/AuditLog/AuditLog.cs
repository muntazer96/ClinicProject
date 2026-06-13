using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.AuditLog
{
    public class AuditLog : BaseEntity<long>
    {
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public Guid? UserId { get; set; }
        public int? DoctorId { get; set; }
        public int? ClinicId { get; set; }
        public int? AppointmentId { get; set; }
        public int? SubscriptionId { get; set; }
        public string? Details { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }
}
