namespace Clinic_Booking.DTOs.AppVersionDTO
{
    public class AppVersionCheckDto
    {
        public string Platform { get; set; } = string.Empty;
        public string CurrentVersion { get; set; } = string.Empty;
        public int CurrentBuildNumber { get; set; }
        public string LatestVersion { get; set; } = string.Empty;
        public int LatestBuildNumber { get; set; }
        public string MinimumSupportedVersion { get; set; } = string.Empty;
        public int MinimumSupportedBuildNumber { get; set; }
        public bool UpdateAvailable { get; set; }
        public bool UpdateRequired { get; set; }
        public bool ForceUpdate { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? UpdateUrl { get; set; }
    }
}
