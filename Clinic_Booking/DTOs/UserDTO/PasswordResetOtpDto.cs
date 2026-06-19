using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.UserDTO
{
    public class PasswordResetOtpRequestDto
    {
        [Required]
        [RegularExpression(@"^07\d{9}$", ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقم ويبدأ بـ 07.")]
        public string PhoneNumber { get; set; }
    }

    public class PasswordResetOtpVerifyDto
    {
        [Required]
        [RegularExpression(@"^07\d{9}$", ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقم ويبدأ بـ 07.")]
        public string PhoneNumber { get; set; }

        [Required]
        public string OtpCode { get; set; }
    }

    public class PasswordResetOtpResultDto
    {
        public string PhoneNumber { get; set; }
        public string ResetToken { get; set; }
    }
}
