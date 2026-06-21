namespace Clinic_Booking.DTOs.AppReleaseDTO
{
    public class AppReleaseListItemDto
    {
        public int Id { get; set; }
        public string VersionName { get; set; } = string.Empty;
        public int VersionCode { get; set; }
        public long FileSize { get; set; }
        public string? ReleaseNotes { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int DownloadCount { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
