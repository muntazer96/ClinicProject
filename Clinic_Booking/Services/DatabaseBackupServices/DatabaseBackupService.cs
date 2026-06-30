using Clinic_Booking.DTOs.DatabaseBuckupDTO;
using Clinic_Booking.IServices.IDatabaseBackupServices;
using Npgsql;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;

namespace Clinic_Booking.Services.DatabaseBackupServices
{
    public class DatabaseBackupService : BackgroundService, IDatabaseBackupService
    {
        private readonly Channel<string> _queue = Channel.CreateUnbounded<string>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
        private readonly Channel<string> _restoreQueue = Channel.CreateUnbounded<string>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
        private readonly SemaphoreSlim _metadataLock = new(1, 1);
        private readonly SemaphoreSlim _operationLock = new(1, 1);
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<DatabaseBackupService> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        public DatabaseBackupService(
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger<DatabaseBackupService> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        public async Task<List<DatabaseBackupResponse>> GetAllAsync(CancellationToken ct = default)
        {
            await _metadataLock.WaitAsync(ct);
            try
            {
                EnsureBackupDirectory();
                var backups = new List<DatabaseBackupResponse>();
                foreach (var metadataPath in Directory.EnumerateFiles(BackupDirectory, "*.json"))
                {
                    var backup = await ReadMetadataFileAsync(metadataPath, ct);
                    if (backup != null) backups.Add(backup);
                }

                return backups.OrderByDescending(item => item.CreatedAt).ToList();
            }
            finally
            {
                _metadataLock.Release();
            }
        }

        public async Task<DatabaseBackupResponse> EnqueueAsync(
            Guid? requestedByUserId,
            string? requestedByUserName,
            string trigger = "Manual",
            CancellationToken ct = default)
        {
            var backup = await CreateBackupRecordAsync(requestedByUserId, requestedByUserName, trigger, ct);
            await _queue.Writer.WriteAsync(backup.Id, ct);
            return backup;
        }

        private async Task<DatabaseBackupResponse> CreateBackupRecordAsync(
            Guid? requestedByUserId,
            string? requestedByUserName,
            string trigger,
            CancellationToken ct)
        {
            var createdAt = DateTime.UtcNow;
            var backup = new DatabaseBackupResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                FileName = $"eyadaty_{createdAt:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}"[..32] + ".backup",
                Status = "Queued",
                Trigger = trigger,
                CreatedAt = createdAt,
                RequestedByUserId = requestedByUserId,
                RequestedByUserName = requestedByUserName
            };

            await WriteMetadataAsync(backup, ct);
            return backup;
        }

        public async Task<DatabaseBackupFile?> GetFileAsync(string id, CancellationToken ct = default)
        {
            var backup = await ReadMetadataAsync(id, ct);
            if (backup == null || backup.Status != "Completed") return null;

            var path = Path.Combine(BackupDirectory, backup.FileName);
            return File.Exists(path)
                ? new DatabaseBackupFile { Path = path, FileName = backup.FileName }
                : null;
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
        {
            await _metadataLock.WaitAsync(ct);
            try
            {
                var metadataPath = MetadataPath(id);
                var backup = await ReadMetadataFileAsync(metadataPath, ct);
                if (backup == null) return false;
                if (backup.Status is "Queued" or "Running")
                    throw new InvalidOperationException("لا يمكن حذف نسخة احتياطية قيد التنفيذ.");
                var restore = await ReadRestoreStateFileAsync(ct);
                if (restore?.BackupId == id && restore.Status is ("Queued" or "Running"))
                    throw new InvalidOperationException("لا يمكن حذف النسخة المستخدمة في عملية استعادة قيد التنفيذ.");

                DeleteFileIfExists(Path.Combine(BackupDirectory, backup.FileName));
                DeleteFileIfExists(metadataPath);
                return true;
            }
            finally
            {
                _metadataLock.Release();
            }
        }

        public Task OpenFolderAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            EnsureBackupDirectory();

            if (!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException("فتح مجلد النسخ مدعوم حالياً على Windows فقط.");

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                ArgumentList = { BackupDirectory },
                UseShellExecute = true
            });

            return Task.CompletedTask;
        }

        public async Task<DatabaseRestoreResponse?> GetRestoreStatusAsync(CancellationToken ct = default)
        {
            await _metadataLock.WaitAsync(ct);
            try
            {
                return await ReadRestoreStateFileAsync(ct);
            }
            finally
            {
                _metadataLock.Release();
            }
        }

        public async Task<DatabaseRestoreResponse> EnqueueRestoreAsync(
            CreateDatabaseRestoreRequest request,
            Guid? requestedByUserId,
            string? requestedByUserName,
            CancellationToken ct = default)
        {
            var currentRestore = await GetRestoreStatusAsync(ct);
            if (currentRestore?.Status is "Queued" or "Running")
                throw new InvalidOperationException("توجد عملية استعادة قيد التنفيذ حالياً.");

            var completedBackups = (await GetAllAsync(ct))
                .Where(backup => backup.Status == "Completed")
                .ToList();
            var backup = request.UseLatest
                ? completedBackups.OrderByDescending(item => item.CompletedAt).FirstOrDefault()
                : completedBackups.FirstOrDefault(item => item.Id == request.BackupId);
            if (backup == null)
                throw new KeyNotFoundException("النسخة الاحتياطية المطلوبة غير موجودة أو غير مكتملة.");

            var backupPath = Path.Combine(BackupDirectory, backup.FileName);
            if (!File.Exists(backupPath))
                throw new FileNotFoundException("ملف النسخة الاحتياطية غير موجود على الخادم.");

            var restore = new DatabaseRestoreResponse
            {
                Id = Guid.NewGuid().ToString("N"),
                BackupId = backup.Id,
                BackupFileName = backup.FileName,
                Status = "Queued",
                CreatedAt = DateTime.UtcNow,
                RequestedByUserId = requestedByUserId,
                RequestedByUserName = requestedByUserName
            };

            await WriteRestoreStateAsync(restore, ct);
            await _restoreQueue.Writer.WriteAsync(restore.Id, ct);
            return restore;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            EnsureBackupDirectory();
            await MarkInterruptedRestoreAsync(stoppingToken);
            var processQueueTask = ProcessQueueAsync(stoppingToken);
            var processRestoreQueueTask = ProcessRestoreQueueAsync(stoppingToken);
            var scheduleTask = ScheduleAsync(stoppingToken);
            await Task.WhenAll(processQueueTask, processRestoreQueueTask, scheduleTask);
        }

        private async Task ProcessQueueAsync(CancellationToken ct)
        {
            await foreach (var id in _queue.Reader.ReadAllAsync(ct))
            {
                try
                {
                    await _operationLock.WaitAsync(ct);
                    try
                    {
                        await CreateBackupAsync(id, ct);
                        await CleanupExpiredBackupsAsync(ct);
                    }
                    finally
                    {
                        _operationLock.Release();
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database backup {BackupId} failed.", id);
                    await MarkFailedAsync(id, ex.Message, CancellationToken.None);
                }
            }
        }

        private async Task ProcessRestoreQueueAsync(CancellationToken ct)
        {
            await foreach (var id in _restoreQueue.Reader.ReadAllAsync(ct))
            {
                try
                {
                    await _operationLock.WaitAsync(ct);
                    try
                    {
                        await RestoreBackupAsync(id, ct);
                    }
                    finally
                    {
                        _operationLock.Release();
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database restore {RestoreId} failed.", id);
                    await MarkRestoreFailedAsync(id, ex.Message, CancellationToken.None);
                }
            }
        }

        private async Task ScheduleAsync(CancellationToken ct)
        {
            var scheduleHours = _configuration.GetValue("DatabaseBackup:ScheduleHours", 0);
            if (scheduleHours <= 0) return;

            using var timer = new PeriodicTimer(TimeSpan.FromHours(scheduleHours));
            while (await timer.WaitForNextTickAsync(ct))
            {
                await EnqueueAsync(null, "System", "Scheduled", ct);
            }
        }

        private async Task CreateBackupAsync(string id, CancellationToken ct)
        {
            var backup = await ReadMetadataAsync(id, ct)
                ?? throw new InvalidOperationException("Backup metadata was not found.");

            backup.Status = "Running";
            backup.StartedAt = DateTime.UtcNow;
            backup.ErrorMessage = null;
            await WriteMetadataAsync(backup, ct);

            var finalPath = Path.Combine(BackupDirectory, backup.FileName);
            var temporaryPath = finalPath + ".part";
            DeleteFileIfExists(temporaryPath);

            var connectionString = _configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Database connection string is missing.");
            var connection = new NpgsqlConnectionStringBuilder(connectionString);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = ResolvePgDumpPath(),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            processStartInfo.ArgumentList.Add("--format=custom");
            processStartInfo.ArgumentList.Add($"--file={temporaryPath}");
            processStartInfo.ArgumentList.Add("--no-password");
            processStartInfo.ArgumentList.Add($"--host={connection.Host}");
            processStartInfo.ArgumentList.Add($"--port={connection.Port}");
            processStartInfo.ArgumentList.Add($"--username={connection.Username}");
            processStartInfo.ArgumentList.Add($"--dbname={connection.Database}");
            processStartInfo.Environment["PGPASSWORD"] = connection.Password;

            using var process = new Process { StartInfo = processStartInfo };
            if (!process.Start())
                throw new InvalidOperationException("تعذر تشغيل أداة النسخ الاحتياطي PostgreSQL.");

            var errorTask = process.StandardError.ReadToEndAsync(ct);
            var outputTask = process.StandardOutput.ReadToEndAsync(ct);
            var timeoutMinutes = _configuration.GetValue("DatabaseBackup:TimeoutMinutes", 30);
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromMinutes(timeoutMinutes));

            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch
            {
                TryKill(process);
                throw;
            }

            var error = await errorTask;
            _ = await outputTask;
            if (process.ExitCode != 0)
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(error)
                    ? "فشل إنشاء النسخة الاحتياطية."
                    : error.Trim());

            File.Move(temporaryPath, finalPath, true);
            backup.Status = "Completed";
            backup.CompletedAt = DateTime.UtcNow;
            backup.SizeBytes = new FileInfo(finalPath).Length;
            await WriteMetadataAsync(backup, ct);
        }

        private async Task MarkFailedAsync(string id, string message, CancellationToken ct)
        {
            var backup = await ReadMetadataAsync(id, ct);
            if (backup == null) return;

            DeleteFileIfExists(Path.Combine(BackupDirectory, backup.FileName + ".part"));
            backup.Status = "Failed";
            backup.CompletedAt = DateTime.UtcNow;
            backup.ErrorMessage = message;
            await WriteMetadataAsync(backup, ct);
        }

        private async Task RestoreBackupAsync(string id, CancellationToken ct)
        {
            var restore = await GetRestoreStatusAsync(ct);
            if (restore == null || restore.Id != id)
                throw new InvalidOperationException("Restore state was not found.");

            restore.Status = "Running";
            restore.StartedAt = DateTime.UtcNow;
            restore.ErrorMessage = null;
            await WriteRestoreStateAsync(restore, ct);

            var safetyBackup = await CreateBackupRecordAsync(
                restore.RequestedByUserId,
                restore.RequestedByUserName,
                "BeforeRestore",
                ct);
            await CreateBackupAsync(safetyBackup.Id, ct);

            var backupPath = Path.Combine(BackupDirectory, restore.BackupFileName);
            if (!File.Exists(backupPath))
                throw new FileNotFoundException("ملف النسخة الاحتياطية غير موجود على الخادم.");

            var connectionString = _configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Database connection string is missing.");
            var connection = new NpgsqlConnectionStringBuilder(connectionString);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = ResolvePgRestorePath(),
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            processStartInfo.ArgumentList.Add("--clean");
            processStartInfo.ArgumentList.Add("--if-exists");
            processStartInfo.ArgumentList.Add("--no-owner");
            processStartInfo.ArgumentList.Add("--no-privileges");
            processStartInfo.ArgumentList.Add("--exit-on-error");
            processStartInfo.ArgumentList.Add("--single-transaction");
            processStartInfo.ArgumentList.Add($"--host={connection.Host}");
            processStartInfo.ArgumentList.Add($"--port={connection.Port}");
            processStartInfo.ArgumentList.Add($"--username={connection.Username}");
            processStartInfo.ArgumentList.Add($"--dbname={connection.Database}");
            processStartInfo.ArgumentList.Add(backupPath);
            processStartInfo.Environment["PGPASSWORD"] = connection.Password;

            using var process = new Process { StartInfo = processStartInfo };
            if (!process.Start())
                throw new InvalidOperationException("تعذر تشغيل أداة استعادة PostgreSQL.");

            var errorTask = process.StandardError.ReadToEndAsync(ct);
            var outputTask = process.StandardOutput.ReadToEndAsync(ct);
            var timeoutMinutes = _configuration.GetValue("DatabaseBackup:RestoreTimeoutMinutes", 60);
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromMinutes(timeoutMinutes));

            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch
            {
                TryKill(process);
                throw;
            }

            var error = await errorTask;
            _ = await outputTask;
            if (process.ExitCode != 0)
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(error)
                    ? "فشلت استعادة قاعدة البيانات."
                    : error.Trim());

            restore.Status = "Completed";
            restore.CompletedAt = DateTime.UtcNow;
            await WriteRestoreStateAsync(restore, ct);
        }

        private async Task MarkRestoreFailedAsync(string id, string message, CancellationToken ct)
        {
            var restore = await GetRestoreStatusAsync(ct);
            if (restore == null || restore.Id != id) return;

            restore.Status = "Failed";
            restore.CompletedAt = DateTime.UtcNow;
            restore.ErrorMessage = message;
            await WriteRestoreStateAsync(restore, ct);
        }

        private async Task MarkInterruptedRestoreAsync(CancellationToken ct)
        {
            var restore = await GetRestoreStatusAsync(ct);
            if (restore?.Status is not ("Queued" or "Running")) return;

            restore.Status = "Failed";
            restore.CompletedAt = DateTime.UtcNow;
            restore.ErrorMessage = "توقفت عملية الاستعادة بسبب إعادة تشغيل الخادم.";
            await WriteRestoreStateAsync(restore, ct);
        }

        private async Task WriteRestoreStateAsync(DatabaseRestoreResponse restore, CancellationToken ct)
        {
            await _metadataLock.WaitAsync(ct);
            try
            {
                EnsureBackupDirectory();
                await using var stream = File.Create(RestoreStatePath);
                await JsonSerializer.SerializeAsync(stream, restore, _jsonOptions, ct);
            }
            finally
            {
                _metadataLock.Release();
            }
        }

        private async Task<DatabaseRestoreResponse?> ReadRestoreStateFileAsync(CancellationToken ct)
        {
            if (!File.Exists(RestoreStatePath)) return null;
            await using var stream = File.OpenRead(RestoreStatePath);
            return await JsonSerializer.DeserializeAsync<DatabaseRestoreResponse>(stream, _jsonOptions, ct);
        }

        private async Task CleanupExpiredBackupsAsync(CancellationToken ct)
        {
            var retentionDays = _configuration.GetValue("DatabaseBackup:RetentionDays", 30);
            if (retentionDays <= 0) return;

            var cutoff = DateTime.UtcNow.AddDays(-retentionDays);
            foreach (var backup in await GetAllAsync(ct))
            {
                if (backup.Status == "Completed" && backup.CompletedAt < cutoff)
                    await DeleteAsync(backup.Id, ct);
            }
        }

        private async Task<DatabaseBackupResponse?> ReadMetadataAsync(string id, CancellationToken ct)
        {
            await _metadataLock.WaitAsync(ct);
            try
            {
                return await ReadMetadataFileAsync(MetadataPath(id), ct);
            }
            finally
            {
                _metadataLock.Release();
            }
        }

        private async Task<DatabaseBackupResponse?> ReadMetadataFileAsync(string path, CancellationToken ct)
        {
            if (!File.Exists(path)) return null;
            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<DatabaseBackupResponse>(stream, _jsonOptions, ct);
        }

        private async Task WriteMetadataAsync(DatabaseBackupResponse backup, CancellationToken ct)
        {
            await _metadataLock.WaitAsync(ct);
            try
            {
                EnsureBackupDirectory();
                await using var stream = File.Create(MetadataPath(backup.Id));
                await JsonSerializer.SerializeAsync(stream, backup, _jsonOptions, ct);
            }
            finally
            {
                _metadataLock.Release();
            }
        }

        private string MetadataPath(string id)
        {
            if (!Guid.TryParseExact(id, "N", out _))
                throw new ArgumentException("Invalid backup identifier.", nameof(id));
            return Path.Combine(BackupDirectory, id + ".json");
        }

        private string BackupDirectory
        {
            get
            {
                var configured = _configuration["DatabaseBackup:Directory"];
                return Path.GetFullPath(string.IsNullOrWhiteSpace(configured)
                    ? Path.Combine(_environment.ContentRootPath, ".appdata", "backups")
                    : Path.IsPathRooted(configured)
                        ? configured
                        : Path.Combine(_environment.ContentRootPath, configured));
            }
        }

        private string RestoreStatePath => Path.Combine(BackupDirectory, "restore-status.state");

        private void EnsureBackupDirectory() => Directory.CreateDirectory(BackupDirectory);

        private string ResolvePgDumpPath()
        {
            var configured = _configuration["DatabaseBackup:PgDumpPath"];
            if (!string.IsNullOrWhiteSpace(configured)) return configured;

            if (OperatingSystem.IsWindows())
            {
                var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                var postgresqlPath = Path.Combine(programFiles, "PostgreSQL");
                if (Directory.Exists(postgresqlPath))
                {
                    var discovered = Directory.EnumerateDirectories(postgresqlPath)
                        .OrderByDescending(path => path)
                        .Select(path => Path.Combine(path, "bin", "pg_dump.exe"))
                        .FirstOrDefault(File.Exists);
                    if (discovered != null) return discovered;
                }
            }

            return "pg_dump";
        }

        private string ResolvePgRestorePath()
        {
            var configured = _configuration["DatabaseBackup:PgRestorePath"];
            if (!string.IsNullOrWhiteSpace(configured)) return configured;

            if (OperatingSystem.IsWindows())
            {
                var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                var postgresqlPath = Path.Combine(programFiles, "PostgreSQL");
                if (Directory.Exists(postgresqlPath))
                {
                    var discovered = Directory.EnumerateDirectories(postgresqlPath)
                        .OrderByDescending(path => path)
                        .Select(path => Path.Combine(path, "bin", "pg_restore.exe"))
                        .FirstOrDefault(File.Exists);
                    if (discovered != null) return discovered;
                }
            }

            return "pg_restore";
        }

        private static void DeleteFileIfExists(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        private static void TryKill(Process process)
        {
            try
            {
                if (!process.HasExited) process.Kill(true);
            }
            catch
            {
                // Process cleanup is best-effort; the original failure is more useful.
            }
        }
    }

}
