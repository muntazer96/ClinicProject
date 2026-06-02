using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.AppointmentDTO
{
    public class BookingDetailsDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string? PatientPhoneNumber { get; set; }
        public DateTime AppointmentDate { get; set; }
        public int QueueNumber { get; set; }
        public AppointmentStatus Status { get; set; }
        public bool IsPhoneConfirmed { get; set; }
        public bool HasReview { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public int ClinicId { get; set; }
        public string ClinicName { get; set; } = string.Empty;
        public string ClinicAddress { get; set; } = string.Empty;
        public string? ClinicPhoneNumber { get; set; }
        public string? MapUrl { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
