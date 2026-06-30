using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.DoctorRequestDTO
{
    public class DoctorRequestDetailsDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string? UserId { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string KnownName { get; set; }
        public string Province { get; set; }
        public DateOnly BirthDay { get; set; }
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; }
        public string IdentityFront { get; set; }
        public string? IdentityBack { get; set; }
        public string Status { get; set; }
        public string? RejectedReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
