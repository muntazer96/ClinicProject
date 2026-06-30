namespace Clinic_Booking.DTOs.DoctorRequestDTO
{
    public class VerifyOtpResponseDto
    {
        public int VerificationTokenId { get; set; }
        public Guid UserId { get; set; }
        public string PhoneNumber { get; set; }
    }
}
