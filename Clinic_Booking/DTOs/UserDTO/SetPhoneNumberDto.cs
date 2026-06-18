using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.UserDTO
{
    public class SetPhoneNumberDto
    {
        [Required]
        [StringLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
