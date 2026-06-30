namespace Clinic_Booking.DTOs.AppReleaseDTO
{
    public class AppReleaseResponseDto
    {
        public int Id { get; set; }
        public string VersionName { get; set; } = string.Empty;
        public int VersionCode { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public string FileSize { get; set; } = "0 B";
        public List<string> ReleaseNotes { get; set; } = [];
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int DownloadCount { get; set; }
    }
}
