using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public class ManualAppointmentDto
    {
        [Range(1, int.MaxValue)]
        public int ClinicId { get; set; }

        public DateTime AppointmentDate { get; set; }

        [Required]
        [StringLength(200)]
        public string PatientName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^07\d{9}$", ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقم ويبدأ بـ 07.")]
        public string PatientPhoneNumber { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
