namespace Clinic_Booking.DTOs.DatabaseBuckupDTO
{
    public class DatabaseBackupResponse
    {
        public string Id { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Trigger { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid? RequestedByUserId { get; set; }
        public string? RequestedByUserName { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class DatabaseBackupFile
    {
        public string Path { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

    public class CreateDatabaseRestoreRequest
    {
        public bool UseLatest { get; set; }
        public string? BackupId { get; set; }
        public string Password { get; set; } = string.Empty;
    }

    public class DatabaseRestoreResponse
    {
        public string Id { get; set; } = string.Empty;
        public string BackupId { get; set; } = string.Empty;
        public string BackupFileName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid? RequestedByUserId { get; set; }
        public string? RequestedByUserName { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
