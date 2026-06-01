namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public class QueueAvailabilityDto
    {
        public int ClinicId { get; set; }
        public DateOnly Date { get; set; }
        public string DayName { get; set; }
        public string DayNormalizedName { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int MaxAppointments { get; set; }
        public int BookedAppointments { get; set; }
        public int RemainingAppointments { get; set; }
        public bool IsAvailable { get; set; }
    }
}
