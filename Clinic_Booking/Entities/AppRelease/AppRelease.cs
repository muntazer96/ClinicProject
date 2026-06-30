using Clinic_Booking.Entities.Shared;

namespace Clinic_Booking.Entities.AppRelease
{
    public class AppRelease : BaseEntity<int>
    {
        public string VersionName { get; set; } = "1.0.0";
        public int VersionCode { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? ReleaseNotes { get; set; }
        public bool IsActive { get; set; } = true;
        public int DownloadCount { get; set; } = 0;
    }
}
