using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public class AddAppointmentDto
    {
        [Range(1, int.MaxValue)]
        public int DoctorId { get; set; }
        [Range(1, int.MaxValue)]
        public int ClinicId { get; set; }
        public DateTime AppointmentDate { get; set; }
        [StringLength(200)]
        public string? GuestName { get; set; }
        [RegularExpression(@"^07\d{9}$", ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقم ويبدأ بـ 07.")]
        public string? GuestPhoneNumber { get; set; }
        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
