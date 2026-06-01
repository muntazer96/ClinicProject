namespace Clinic_Booking.DTOs.DoctorAvailabilityDTO
{
    public class UpdateSingleDayAvailabilityDto
    {
        public int ClinicId { get; set; }
        public int DayId { get; set; } 
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxAppointments { get; set; }
        public bool IsAvailable { get; set; }
    }
}
