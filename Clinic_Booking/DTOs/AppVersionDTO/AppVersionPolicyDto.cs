namespace Clinic_Booking.DTOs.AppVersionDTO
{
    public class AppVersionPolicyDto
    {
        public int Id { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string LatestVersion { get; set; } = string.Empty;
        public int LatestBuildNumber { get; set; }
        public string MinimumSupportedVersion { get; set; } = string.Empty;
        public int MinimumSupportedBuildNumber { get; set; }
        public bool ForceUpdate { get; set; }
        public bool IsEnabled { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? UpdateUrl { get; set; }
    }
}
