using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.DoctorRequestDTO
{
    public class AcceptRequestDto
    {
        [Required]
        public int RequestId { get; set; }
    }
}
