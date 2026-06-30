using Clinic_Booking.DTOs.DatabaseBuckupDTO;

namespace Clinic_Booking.IServices.IDatabaseBackupServices
{
    public interface IDatabaseBackupService
    {
        Task<List<DatabaseBackupResponse>> GetAllAsync(CancellationToken ct = default);
        Task<DatabaseBackupResponse> EnqueueAsync(
            Guid? requestedByUserId,
            string? requestedByUserName,
            string trigger = "Manual",
            CancellationToken ct = default);
        Task<DatabaseBackupFile?> GetFileAsync(string id, CancellationToken ct = default);
        Task<bool> DeleteAsync(string id, CancellationToken ct = default);
        Task OpenFolderAsync(CancellationToken ct = default);
        Task<DatabaseRestoreResponse?> GetRestoreStatusAsync(CancellationToken ct = default);
        Task<DatabaseRestoreResponse> EnqueueRestoreAsync(
            CreateDatabaseRestoreRequest request,
            Guid? requestedByUserId,
            string? requestedByUserName,
            CancellationToken ct = default);
    }
}
