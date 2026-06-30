using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.UserDTO
{
    public class ConfirmPhoneDto
    {
        [Required]
        public string OtpCode { get; set; }
    }
}
