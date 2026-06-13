using Clinic_Booking.Data;
using Clinic_Booking.Entities.DoctorSubscription;
using Clinic_Booking.Entities.Notification;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.IPushNotificationServices;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.SubscriptionExpirationServices
{
    public class SubscriptionExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SubscriptionExpirationService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Task RunOnceAsync(CancellationToken cancellationToken = default)
        {
            return ExpireSubscriptionsAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await RunOnceAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task ExpireSubscriptionsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var push = scope.ServiceProvider.GetRequiredService<IPushNotificationServices>();
            var now = DateTime.UtcNow;

            await SendExpirationWarningsAsync(context, push, now, cancellationToken);

            var expiredSubscriptions = await context.DoctorSubscriptions
                .Where(subscription =>
                    subscription.Status == SubscriptionStatus.Active &&
                    subscription.EndDate < now)
                .ToListAsync(cancellationToken);
            if (expiredSubscriptions.Count == 0)
            {
                return;
            }

            var doctorIds = expiredSubscriptions
                .Select(subscription => subscription.DoctorId)
                .Distinct()
                .ToList();
            foreach (var subscription in expiredSubscriptions)
            {
                subscription.Status = SubscriptionStatus.Expired;
                subscription.ModifiedAt = now;
            }

            var doctorsWithAnotherActiveSubscription = await context.DoctorSubscriptions
                .Where(subscription =>
                    doctorIds.Contains(subscription.DoctorId) &&
                    subscription.Status == SubscriptionStatus.Active &&
                    subscription.StartDate <= now &&
                    subscription.EndDate >= now)
                .Select(subscription => subscription.DoctorId)
                .Distinct()
                .ToListAsync(cancellationToken);

            var doctorIdsToDisable = doctorIds.Except(doctorsWithAnotherActiveSubscription).ToList();
            var features = await context.DoctorFeature
                .Where(feature =>
                    doctorIdsToDisable.Contains(feature.DoctorId) &&
                    feature.IsEnabled &&
                    !feature.IsDeleted)
                .ToListAsync(cancellationToken);
            foreach (var feature in features)
            {
                feature.IsEnabled = false;
                feature.ModifiedAt = now;
            }

            var basicPackage = await context.SubscriptionPackages
                .Where(package => package.NormalizedName == "Basic")
                .FirstOrDefaultAsync(cancellationToken);

            if (basicPackage != null)
            {
                foreach (var doctorId in doctorIdsToDisable)
                {
                    context.DoctorSubscriptions.Add(new DoctorSubscription
                    {
                        DoctorId = doctorId,
                        PackageId = basicPackage.Id,
                        StartDate = now,
                        EndDate = now.AddYears(100),
                        Status = SubscriptionStatus.Active,
                    });
                }
            }

            await context.SaveChangesAsync(cancellationToken);

            foreach (var doctorId in doctorIdsToDisable)
            {
                await NotifyDoctorAsync(
                    context,
                    push,
                    doctorId,
                    "انتهى الاشتراك",
                    "تمت إعادة الحساب إلى الخطة المجانية وإيقاف الميزات المدفوعة.",
                    cancellationToken);
            }
        }

        private static async Task SendExpirationWarningsAsync(
            ApplicationDbContext context,
            IPushNotificationServices push,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var today = DateOnly.FromDateTime(now);
            var activeSubscriptions = await context.DoctorSubscriptions
                .Include(subscription => subscription.Package)
                .Where(subscription =>
                    subscription.Status == SubscriptionStatus.Active &&
                    subscription.EndDate >= now &&
                    subscription.EndDate <= now.AddDays(3))
                .ToListAsync(cancellationToken);

            foreach (var subscription in activeSubscriptions)
            {
                var endDate = DateOnly.FromDateTime(subscription.EndDate);
                var daysLeft = endDate.DayNumber - today.DayNumber;
                if (daysLeft is < 1 or > 3)
                {
                    continue;
                }

                var marker = $"subscription-expiry:{subscription.Id}:{daysLeft}";
                var alreadySent = await context.Notifications.AnyAsync(
                    notification =>
                        notification.DoctorId == subscription.DoctorId &&
                        notification.Message.Contains(marker) &&
                        !notification.IsDeleted,
                    cancellationToken);
                if (alreadySent)
                {
                    continue;
                }

                await NotifyDoctorAsync(
                    context,
                    push,
                    subscription.DoctorId,
                    "قرب انتهاء الاشتراك",
                    $"تبقى {daysLeft} يوم على انتهاء اشتراك {subscription.Package.Name}. [{marker}]",
                    cancellationToken);
            }
        }

        private static async Task NotifyDoctorAsync(
            ApplicationDbContext context,
            IPushNotificationServices push,
            int doctorId,
            string title,
            string body,
            CancellationToken cancellationToken)
        {
            context.Notifications.Add(new Notification
            {
                DoctorId = doctorId,
                Message = body,
                CreatedAt = DateTime.UtcNow,
                Status = NotificationStatus.Unread
            });
            await context.SaveChangesAsync(cancellationToken);

            var doctorUserId = await context.Doctors
                .Where(doctor => doctor.Id == doctorId && doctor.UserId.HasValue && !doctor.IsDeleted)
                .Select(doctor => doctor.UserId)
                .FirstOrDefaultAsync(cancellationToken);
            if (doctorUserId.HasValue)
            {
                await push.SendToUserAsync(
                    doctorUserId.Value,
                    title,
                    body,
                    new Dictionary<string, string>
                    {
                        ["type"] = "subscription",
                        ["doctorId"] = doctorId.ToString()
                    },
                    cancellationToken);
            }
        }
    }
}
