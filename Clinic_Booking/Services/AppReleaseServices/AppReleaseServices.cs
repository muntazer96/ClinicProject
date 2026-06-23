using Clinic_Booking.Data;
using Clinic_Booking.DTOs.AppReleaseDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.AppRelease;
using Clinic_Booking.IServices.IAppReleaseServices;
using Clinic_Booking.IServices.ILoadServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Services.AppReleaseServices
{
    public class AppReleaseServices : IAppReleaseServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AppReleaseServices(ApplicationDbContext context, ILoadServices load, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _load = load;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> UploadAsync(CreateAppReleaseDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "يرجى اختيار ملف."
                });
            }

            var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
            if (extension != ".apk")
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "يُسمح فقط بملفات APK."
                });
            }

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "AppDownloads");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}.apk";
            var filePath = Path.Combine(folderPath, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            var release = new AppRelease
            {
                VersionName = dto.VersionName,
                VersionCode = dto.VersionCode,
                FileName = fileName,
                FileSize = dto.File.Length,
                ReleaseNotes = dto.ReleaseNotes,
                IsActive = dto.IsActive,
                DownloadCount = 0,
                CreatorId = _load.GetCurrentUserId(),
                CreatedAt = BusinessClock.Now()
            };

            if (release.IsActive)
            {
                var activeReleases = await _context.Set<AppRelease>()
                    .Where(r => r.IsActive && !r.IsDeleted)
                    .ToListAsync();

                foreach (var ar in activeReleases)
                {
                    ar.IsActive = false;
                }
            }

            _context.Set<AppRelease>().Add(release);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<AppReleaseListItemDto>
            {
                Status = "Success",
                Code = 200,
                Message = "تم رفع الإصدار بنجاح.",
                Data = MapToListItem(release)
            });
        }

        public async Task<IActionResult> GetLatestAsync()
        {
            var release = await _context.Set<AppRelease>()
                .Where(r => r.IsActive && !r.IsDeleted)
                .OrderByDescending(r => r.VersionCode)
                .FirstOrDefaultAsync();

            if (release == null)
            {
                return new OkObjectResult(new ResponseDto<AppReleaseResponseDto>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "لا يوجد إصدار متاح حالياً."
                });
            }

            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = request is not null
                ? $"{request.Scheme}://{request.Host}"
                : "https://localhost:7136";
            var dto = new AppReleaseResponseDto
            {
                Id = release.Id,
                VersionName = release.VersionName,
                VersionCode = release.VersionCode,
                DownloadUrl = $"{baseUrl}/api/app-release/download",
                FileSize = FormatFileSize(release.FileSize),
                ReleaseNotes = (release.ReleaseNotes ?? "")
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim().TrimStart('-', '•', '*', ' '))
                    .Where(l => l.Length > 0)
                    .ToList(),
                IsActive = release.IsActive,
                CreatedAt = release.CreatedAt,
                DownloadCount = release.DownloadCount
            };

            return new OkObjectResult(new ResponseDto<AppReleaseResponseDto>
            {
                Status = "Success",
                Code = 200,
                Message = "تم العثور على الإصدار.",
                Data = dto
            });
        }

        public async Task<IActionResult> DownloadAsync()
        {
            var release = await _context.Set<AppRelease>()
                .Where(r => r.IsActive && !r.IsDeleted)
                .OrderByDescending(r => r.VersionCode)
                .FirstOrDefaultAsync();

            if (release == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "لا يوجد إصدار متاح للتحميل."
                });
            }

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "AppDownloads");
            var filePath = Path.Combine(folderPath, release.FileName);

            if (!System.IO.File.Exists(filePath))
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "ملف التطبيق غير موجود على الخادم."
                });
            }

            release.DownloadCount++;
            await _context.SaveChangesAsync();

            var memory = new MemoryStream();
            await using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return new FileContentResult(memory.ToArray(), "application/vnd.android.package-archive")
            {
                FileDownloadName = $"ClinicApp-{release.VersionName}.apk"
            };
        }

        public async Task<IActionResult> GetListAsync()
        {
            var releases = await _context.Set<AppRelease>()
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var userIds = releases
                .Where(r => r.CreatorId.HasValue)
                .Select(r => r.CreatorId!.Value)
                .Distinct()
                .ToList();

            var users = await _context.AspNetUsers
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? u.Email ?? "---");

            var dtos = releases.Select(r => new AppReleaseListItemDto
            {
                Id = r.Id,
                VersionName = r.VersionName,
                VersionCode = r.VersionCode,
                FileSize = r.FileSize,
                ReleaseNotes = r.ReleaseNotes,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                DownloadCount = r.DownloadCount,
                FileName = r.FileName,
                CreatedBy = r.CreatorId.HasValue && users.TryGetValue(r.CreatorId.Value, out var name)
                    ? name
                    : "---"
            }).ToList();

            return new OkObjectResult(new ResponseDto<List<AppReleaseListItemDto>>
            {
                Status = "Success",
                Code = 200,
                Message = "تم جلب الإصدارات.",
                Data = dtos
            });
        }

        public async Task<IActionResult> ToggleActiveAsync(int id)
        {
            var release = await _context.Set<AppRelease>()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (release == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "الإصدار غير موجود."
                });
            }

            if (!release.IsActive)
            {
                var activeReleases = await _context.Set<AppRelease>()
                    .Where(r => r.IsActive && !r.IsDeleted && r.Id != id)
                    .ToListAsync();

                foreach (var ar in activeReleases)
                {
                    ar.IsActive = false;
                    ar.ModifierId = _load.GetCurrentUserId();
                    ar.ModifiedAt = BusinessClock.Now();
                }

                release.IsActive = true;
            }
            else
            {
                release.IsActive = false;
            }

            release.ModifierId = _load.GetCurrentUserId();
            release.ModifiedAt = BusinessClock.Now();

            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<string>
            {
                Status = "Success",
                Code = 200,
                Message = release.IsActive ? "تم تفعيل الإصدار." : "تم إلغاء تفعيل الإصدار."
            });
        }

        public async Task<IActionResult> DeleteAsync(int id)
        {
            var release = await _context.Set<AppRelease>()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (release == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "الإصدار غير موجود."
                });
            }

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "AppDownloads");
            var filePath = Path.Combine(folderPath, release.FileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            release.IsDeleted = true;
            release.DeleterId = _load.GetCurrentUserId();
            release.DeletedAt = BusinessClock.Now();

            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<string>
            {
                Status = "Success",
                Code = 200,
                Message = "تم حذف الإصدار."
            });
        }

        private static AppReleaseListItemDto MapToListItem(AppRelease r)
        {
            return new AppReleaseListItemDto
            {
                Id = r.Id,
                VersionName = r.VersionName,
                VersionCode = r.VersionCode,
                FileSize = r.FileSize,
                ReleaseNotes = r.ReleaseNotes,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                DownloadCount = r.DownloadCount,
                FileName = r.FileName
            };
        }

        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = ["B", "KB", "MB", "GB"];
            int order = 0;
            double size = bytes;
            while (size >= 1024 && order < suffixes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            return $"{size:0.##} {suffixes[order]}";
        }

    }
}
