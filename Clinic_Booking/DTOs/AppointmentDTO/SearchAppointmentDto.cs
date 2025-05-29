using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public class SearchAppointmentDto
    {
        public AppointmentStatus? Status { get; set; }
        public Guid? UserId { get; set; }
        public int? DoctorId { get; set; }
    }
}
