using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.UserDTO
{
    public class SignUpDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        [Required]
        //[Phone]
        [StringLength(30)]
        public string PhoneNumber { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
