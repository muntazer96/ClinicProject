using Clinic_Booking.Entities.AppVersion;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Data
{
    public static class AppVersionPolicySeeder
    {
        private static readonly AppVersionPolicy[] DefaultPolicies =
        [
            Create("android", "تحديث جديد متوفر", "تتوفر نسخة أحدث من التطبيق. يرجى التحديث للحصول على أفضل تجربة."),
            Create("ios", "تحديث جديد متوفر", "تتوفر نسخة أحدث من التطبيق. يرجى التحديث للحصول على أفضل تجربة."),
            Create("web", "تحديث جديد متوفر", "تتوفر نسخة أحدث من التطبيق. يرجى التحديث للحصول على أفضل تجربة."),
            Create("admin", "تحديث لوحة التحكم", "تتوفر نسخة أحدث من لوحة التحكم."),
            Create("backend", "تحديث الخادم", "تم تسجيل نسخة أحدث من الخادم.")
        ];

        public static async Task SeedAsync(ApplicationDbContext context)
        {
            foreach (var defaultPolicy in DefaultPolicies)
            {
                var existing = await context.AppVersionPolicies
                    .FirstOrDefaultAsync(policy => policy.Platform == defaultPolicy.Platform);

                if (existing == null)
                {
                    await context.AppVersionPolicies.AddAsync(defaultPolicy);
                    continue;
                }

                if (IsOldInitialDefault(existing))
                {
                    existing.LatestVersion = "0.0.0";
                    existing.LatestBuildNumber = 0;
                    existing.MinimumSupportedVersion = "0.0.0";
                    existing.MinimumSupportedBuildNumber = 0;
                    existing.ModifiedAt = BusinessClock.Now();
                }
            }

            await context.SaveChangesAsync();
        }

        private static AppVersionPolicy Create(string platform, string title, string message) => new()
        {
            Platform = platform,
            LatestVersion = "0.0.0",
            LatestBuildNumber = 0,
            MinimumSupportedVersion = "0.0.0",
            MinimumSupportedBuildNumber = 0,
            ForceUpdate = false,
            IsEnabled = true,
            Title = title,
            Message = message,
            CreatedAt = new DateTime(2026, 6, 5, 0, 0, 0, DateTimeKind.Unspecified)
        };

        private static bool IsOldInitialDefault(AppVersionPolicy policy) =>
            policy.LatestVersion == "1.0.0" &&
            policy.LatestBuildNumber == 1 &&
            policy.MinimumSupportedVersion == "1.0.0" &&
            policy.MinimumSupportedBuildNumber == 1 &&
            string.IsNullOrWhiteSpace(policy.UpdateUrl);
    }
}
