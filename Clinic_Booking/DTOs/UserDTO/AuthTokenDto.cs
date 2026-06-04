namespace Clinic_Booking.DTOs.UserDTO
{
    public class AuthTokenDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime TokenExpiresAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}
