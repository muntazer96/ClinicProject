using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.ClinicException
{
    public class ClinicException : BaseEntity<int>
    {
        public int ClinicId { get; set; }
        public Clinic.Clinic Clinic { get; set; }
        public DateTime ExceptionDate { get; set; }
        public bool IsClosed { get; set; }
        public string? ClosureReason { get; set; }
        public int? MaxAppointments { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }
}
