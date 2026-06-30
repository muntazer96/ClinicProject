using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.UserDTO
{
    public class DeviceTokenDto
    {
        [Required]
        [StringLength(2048)]
        public string Token { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string Platform { get; set; } = string.Empty;

        [StringLength(200)]
        public string? DeviceId { get; set; }
    }
}
