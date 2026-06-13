using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public class CancelMyAppointmentDto
    {
        [Range(1, int.MaxValue)]
        public int AppointmentId { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }
    }
}
