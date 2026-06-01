using Clinic_Booking.Data;
using Clinic_Booking.Enums;
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ExpireSubscriptionsAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task ExpireSubscriptionsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTime.UtcNow;

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

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
