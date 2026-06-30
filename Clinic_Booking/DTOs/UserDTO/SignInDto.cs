using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.UserDTO
{
    public class SignInDto
    {
        [Required]
        //[RegularExpression(@"^07\d{9}$", ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقم ويبدأ بـ 07.")]
        public string PhoneNumber { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
