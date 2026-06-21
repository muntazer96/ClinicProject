using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.AppReleaseDTO
{
    public class CreateAppReleaseDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        public string VersionName { get; set; } = "1.0.0";

        [Range(1, int.MaxValue)]
        public int VersionCode { get; set; }

        [MaxLength(2000)]
        public string? ReleaseNotes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
