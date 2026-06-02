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
        [Phone]
        [StringLength(30)]
        public string PatientPhoneNumber { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
