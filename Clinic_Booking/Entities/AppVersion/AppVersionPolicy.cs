using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.AppVersion
{
    public class AppVersionPolicy : BaseEntity<int>
    {
        public string Platform { get; set; } = string.Empty;
        public string LatestVersion { get; set; } = "0.0.0";
        public int LatestBuildNumber { get; set; }
        public string MinimumSupportedVersion { get; set; } = "0.0.0";
        public int MinimumSupportedBuildNumber { get; set; }
        public bool ForceUpdate { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string Title { get; set; } = "تحديث جديد متوفر";
        public string Message { get; set; } = "تتوفر نسخة أحدث من التطبيق. يرجى التحديث للحصول على أفضل تجربة.";
        public string? UpdateUrl { get; set; }
    }
}
