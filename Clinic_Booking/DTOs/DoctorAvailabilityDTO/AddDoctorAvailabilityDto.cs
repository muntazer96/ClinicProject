namespace Clinic_Booking.DTOs.DoctorAvailabilityDTO
{
    public class AddDoctorAvailabilityDto
    {
        public int ClinicId { get; set; }

        public List<DailyAvailabilityDto> Days { get; set; }
    }

    public class DailyAvailabilityDto
    {
        public int DayId { get; set; } // 1 = الأحد، 2 = الاثنين، ...
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxAppointments { get; set; }
    }
}
