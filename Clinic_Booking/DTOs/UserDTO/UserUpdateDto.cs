using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.UserDTO
{
    public class UserUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(30)]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
