namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public class AddAppointmentDto
    {
        //public Guid UserId { get; set; }
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
    }
}
