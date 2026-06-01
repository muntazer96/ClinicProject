using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.UserDTO
{
    public class ResetPasswordDto
    {
        public Guid UserId { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }
}
