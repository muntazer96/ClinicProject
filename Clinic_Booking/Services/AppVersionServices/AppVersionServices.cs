using Clinic_Booking.Data;
using Clinic_Booking.DTOs.AppVersionDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.AppVersion;
using Clinic_Booking.IServices.IAppVersionServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.AppVersionServices
{
    public class AppVersionServices : IAppVersionServices
    {
        private readonly ApplicationDbContext _context;

        public AppVersionServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> GetListAsync()
        {
            var policies = await _context.AppVersionPolicies
                .Where(policy => !policy.IsDeleted)
                .OrderBy(policy => policy.Platform)
                .Select(policy => ToDto(policy))
                .ToListAsync();

            return new OkObjectResult(new ResponseDto<List<AppVersionPolicyDto>>
            {
                Status = "Success",
                Code = 200,
                Message = "تم جلب سياسات الإصدارات بنجاح.",
                Data = policies
            });
        }

        public async Task<IActionResult> CheckAsync(string platform, string currentVersion, int currentBuildNumber)
        {
            var normalizedPlatform = NormalizePlatform(platform);
            var policy = await _context.AppVersionPolicies
                .Where(item => !item.IsDeleted && item.IsEnabled && item.Platform == normalizedPlatform)
                .FirstOrDefaultAsync();

            if (policy == null)
            {
                return new OkObjectResult(new ResponseDto<AppVersionCheckDto>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "لا توجد سياسة تحديث فعالة لهذه المنصة.",
                    Data = new AppVersionCheckDto
                    {
                        Platform = normalizedPlatform,
                        CurrentVersion = currentVersion,
                        CurrentBuildNumber = currentBuildNumber,
                        LatestVersion = currentVersion,
                        LatestBuildNumber = currentBuildNumber,
                        MinimumSupportedVersion = currentVersion,
                        MinimumSupportedBuildNumber = currentBuildNumber
                    }
                });
            }

            var belowMinimum = IsNewer(policy.MinimumSupportedVersion, policy.MinimumSupportedBuildNumber, currentVersion, currentBuildNumber);
            var responseLatestVersion = policy.LatestVersion;
            var responseLatestBuildNumber = policy.LatestBuildNumber;

            if (IsNewer(policy.MinimumSupportedVersion, policy.MinimumSupportedBuildNumber, responseLatestVersion, responseLatestBuildNumber))
            {
                responseLatestVersion = policy.MinimumSupportedVersion;
                responseLatestBuildNumber = policy.MinimumSupportedBuildNumber;
            }

            var updateAvailable = IsNewer(responseLatestVersion, responseLatestBuildNumber, currentVersion, currentBuildNumber);
            var updateRequired = belowMinimum || (policy.ForceUpdate && updateAvailable);

            return new OkObjectResult(new ResponseDto<AppVersionCheckDto>
            {
                Status = "Success",
                Code = 200,
                Message = "تم فحص إصدار التطبيق بنجاح.",
                Data = new AppVersionCheckDto
                {
                    Platform = policy.Platform,
                    CurrentVersion = currentVersion,
                    CurrentBuildNumber = currentBuildNumber,
                    LatestVersion = responseLatestVersion,
                    LatestBuildNumber = responseLatestBuildNumber,
                    MinimumSupportedVersion = policy.MinimumSupportedVersion,
                    MinimumSupportedBuildNumber = policy.MinimumSupportedBuildNumber,
                    UpdateAvailable = updateAvailable,
                    UpdateRequired = updateRequired,
                    ForceUpdate = policy.ForceUpdate,
                    Title = policy.Title,
                    Message = policy.Message,
                    UpdateUrl = policy.UpdateUrl
                }
            });
        }

        public async Task<IActionResult> UpsertAsync(UpdateAppVersionPolicyDto dto)
        {
            var platform = NormalizePlatform(dto.Platform);
            var policy = await _context.AppVersionPolicies
                .FirstOrDefaultAsync(item => item.Platform == platform && !item.IsDeleted);

            if (policy == null)
            {
                policy = new AppVersionPolicy { Platform = platform };
                await _context.AppVersionPolicies.AddAsync(policy);
            }

            policy.LatestVersion = dto.LatestVersion.Trim();
            policy.LatestBuildNumber = dto.LatestBuildNumber;
            policy.MinimumSupportedVersion = dto.MinimumSupportedVersion.Trim();
            policy.MinimumSupportedBuildNumber = dto.MinimumSupportedBuildNumber;
            policy.ForceUpdate = dto.ForceUpdate;
            policy.IsEnabled = dto.IsEnabled;
            policy.Title = dto.Title.Trim();
            policy.Message = dto.Message.Trim();
            policy.UpdateUrl = string.IsNullOrWhiteSpace(dto.UpdateUrl) ? null : dto.UpdateUrl.Trim();
            policy.ModifiedAt = BusinessClock.Now();

            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<AppVersionPolicyDto>
            {
                Status = "Success",
                Code = 200,
                Message = "تم حفظ سياسة إصدار التطبيق بنجاح.",
                Data = ToDto(policy)
            });
        }

        private static AppVersionPolicyDto ToDto(AppVersionPolicy policy) => new()
        {
            Id = policy.Id,
            Platform = policy.Platform,
            LatestVersion = policy.LatestVersion,
            LatestBuildNumber = policy.LatestBuildNumber,
            MinimumSupportedVersion = policy.MinimumSupportedVersion,
            MinimumSupportedBuildNumber = policy.MinimumSupportedBuildNumber,
            ForceUpdate = policy.ForceUpdate,
            IsEnabled = policy.IsEnabled,
            Title = policy.Title,
            Message = policy.Message,
            UpdateUrl = policy.UpdateUrl
        };

        private static string NormalizePlatform(string platform) =>
            string.IsNullOrWhiteSpace(platform) ? "android" : platform.Trim().ToLowerInvariant();

        private static bool IsNewer(string targetVersion, int targetBuild, string currentVersion, int currentBuild)
        {
            var versionCompare = CompareVersions(targetVersion, currentVersion);
            if (versionCompare != 0)
            {
                return versionCompare > 0;
            }

            return targetBuild > currentBuild;
        }

        private static int CompareVersions(string left, string right)
        {
            var leftParts = ParseVersion(left);
            var rightParts = ParseVersion(right);
            var length = Math.Max(leftParts.Length, rightParts.Length);

            for (var index = 0; index < length; index++)
            {
                var leftValue = index < leftParts.Length ? leftParts[index] : 0;
                var rightValue = index < rightParts.Length ? rightParts[index] : 0;
                if (leftValue != rightValue)
                {
                    return leftValue.CompareTo(rightValue);
                }
            }

            return 0;
        }

        private static int[] ParseVersion(string version) =>
            (version ?? string.Empty)
                .Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => int.TryParse(part, out var value) ? value : 0)
                .ToArray();
    }
}
