using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public class SearchAppointmentDto
    {
        public int? Id { get; set; }

        public Guid? UserId { get; set; }
        public string? UserFullName { get; set; }
        public int? DoctorId { get; set; }
        public int? ClinicId { get; set; }
        public string? DoctorFullName { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public AppointmentStatus? Status { get; set; }

    }
}
