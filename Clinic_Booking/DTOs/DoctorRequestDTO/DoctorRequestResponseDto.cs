namespace Clinic_Booking.DTOs.DoctorRequestDTO
{
    public class DoctorRequestResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; }
    }
}
