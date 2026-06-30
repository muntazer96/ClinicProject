using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.DoctorRequestDTO
{
    public class VerifyDoctorRequestOtpDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [RegularExpression(@"^07\d{9}$", ErrorMessage = "رقم الهاتف غير صحيح.")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; }
    }
}
