using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.DoctorRequestDTO
{
    public class RejectRequestDto
    {
        [Required]
        public int RequestId { get; set; }

        [Required]
        [StringLength(500)]
        public string RejectedReason { get; set; }
    }
}
