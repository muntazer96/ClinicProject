using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.Analytics
{
    public class AnalyticsEvent : BaseEntity<long>
    {
        public string EventType { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public bool IsGuest { get; set; }
        public int? DoctorId { get; set; }
        public int? ClinicId { get; set; }
        public int? SpecializationId { get; set; }
        public int? AppointmentId { get; set; }
        public int? OfferId { get; set; }
        public string? Source { get; set; }
        public string? Platform { get; set; }
        public string? Page { get; set; }
        public string? Province { get; set; }
        public string? SearchText { get; set; }
        public string? SessionId { get; set; }
        public DateTime OccurredAt { get; set; } = BusinessClock.Now();
    }
}
