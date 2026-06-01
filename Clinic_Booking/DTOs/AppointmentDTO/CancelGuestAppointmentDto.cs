namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public class CancelGuestAppointmentDto
    {
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
        public string? Reason { get; set; }
    }
}
