using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.UserDTO
{
    public class ResetPasswordDto
    {
        [Required]
        [Phone]
        [StringLength(30)]
        public string PhoneNumber { get; set; }

        [Required]
        public string ResetToken { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }
}
