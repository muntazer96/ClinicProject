using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.UserDTO
{
    public class PasswordResetOtpRequestDto
    {
        [Required]
        [Phone]
        [StringLength(30)]
        public string PhoneNumber { get; set; }
    }

    public class PasswordResetOtpVerifyDto
    {
        [Required]
        [Phone]
        [StringLength(30)]
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
