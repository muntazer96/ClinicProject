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
        [Phone]
        [StringLength(30)]
        public string? GuestPhoneNumber { get; set; }
        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
