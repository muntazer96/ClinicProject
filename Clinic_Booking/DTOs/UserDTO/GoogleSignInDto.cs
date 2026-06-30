using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.UserDTO
{
    public class GoogleSignInDto
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}
