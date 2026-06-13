using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public class CancelGuestAppointmentDto
    {
        [Required]
        [Phone]
        [StringLength(30)]
        public string PhoneNumber { get; set; }

        [Required]
        public string Code { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }
    }
}
