using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.DoctorRequestDTO
{
    public class CheckPhoneRequestDto
    {
        [Required]
        public string CaptchaToken { get; set; }

        [Required]
        [RegularExpression(@"^07\d{9}$", ErrorMessage = "رقم الهاتف غير صحيح.")]
        public string PhoneNumber { get; set; }
    }
}
