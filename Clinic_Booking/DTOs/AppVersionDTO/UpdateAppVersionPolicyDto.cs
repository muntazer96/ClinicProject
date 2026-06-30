using System.ComponentModel.DataAnnotations;

namespace Clinic_Booking.DTOs.AppVersionDTO
{
    public class UpdateAppVersionPolicyDto
    {
        [Required]
        [MaxLength(40)]
        public string Platform { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string LatestVersion { get; set; } = "0.0.0";

        [Range(0, int.MaxValue)]
        public int LatestBuildNumber { get; set; }

        [Required]
        [MaxLength(30)]
        public string MinimumSupportedVersion { get; set; } = "0.0.0";

        [Range(0, int.MaxValue)]
        public int MinimumSupportedBuildNumber { get; set; }

        public bool ForceUpdate { get; set; }
        public bool IsEnabled { get; set; } = true;

        [Required]
        [MaxLength(120)]
        public string Title { get; set; } = "تحديث جديد متوفر";

        [Required]
        [MaxLength(600)]
        public string Message { get; set; } = "تتوفر نسخة أحدث من التطبيق. يرجى التحديث للحصول على أفضل تجربة.";

        [MaxLength(500)]
        public string? UpdateUrl { get; set; }
    }
}
