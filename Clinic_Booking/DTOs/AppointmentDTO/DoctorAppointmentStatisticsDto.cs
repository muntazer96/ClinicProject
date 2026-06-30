namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public class DoctorAppointmentStatisticsDto
    {
        public int TotalToday { get; set; }
        public int Pending { get; set; }
        public int Confirmed { get; set; }
        public int Cancelled { get; set; }
        public int Completed { get; set; }
    }
}
