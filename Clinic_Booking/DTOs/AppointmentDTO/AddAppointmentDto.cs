namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public class AddAppointmentDto
    {
        public int DoctorId { get; set; }
        public int ClinicId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? GuestName { get; set; }
        public string? GuestPhoneNumber { get; set; }
        public string? Notes { get; set; }
    }
}
