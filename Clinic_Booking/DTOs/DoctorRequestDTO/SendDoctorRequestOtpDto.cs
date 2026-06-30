using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.DoctorRequestDTO
{
    public class SendDoctorRequestOtpDto
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [RegularExpression(@"^07\d{9}$", ErrorMessage = "رقم الهاتف غير صحيح.")]
        public string PhoneNumber { get; set; }
    }
}
