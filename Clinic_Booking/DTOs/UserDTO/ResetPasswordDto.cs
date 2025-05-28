namespace Clinic_Booking.DTOs.UserDTO
{
    public class ResetPasswordDto
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
