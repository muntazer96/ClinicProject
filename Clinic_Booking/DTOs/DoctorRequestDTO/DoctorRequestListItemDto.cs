using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.DoctorRequestDTO
{
    public class DoctorRequestListItemDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public string KnownName { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public string SpecializationName { get; set; }
        public string ProvinceName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RejectedReason { get; set; }
    }
}
