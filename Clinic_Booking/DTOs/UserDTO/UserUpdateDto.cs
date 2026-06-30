using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.UserDTO
{
    public class UserUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [RegularExpression(@"^07\d{9}$", ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقم ويبدأ بـ 07.")]
        public string PhoneNumber { get; set; }

    }
}
