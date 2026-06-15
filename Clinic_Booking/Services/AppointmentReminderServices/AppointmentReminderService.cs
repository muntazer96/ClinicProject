using Clinic_Booking.Configuration;
using Clinic_Booking.Data;
using Clinic_Booking.Entities.Notification;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.IPushNotificationServices;
using Clinic_Booking.Services.NotificationDeliveryServices;
using Clinic_Booking.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Clinic_Booking.Services.AppointmentReminderServices
{
    public class AppointmentReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AppointmentReminderOptions _options;
        private readonly ILogger<AppointmentReminderService> _logger;
        private DateOnly? _lastRunDate;

        public AppointmentReminderService(
            IServiceScopeFactory scopeFactory,
            IOptions<AppointmentReminderOptions> options,
            ILogger<AppointmentReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_options.Enabled && ShouldRunNow())
                {
                    await SendTodayRemindersAsync(stoppingToken);
                    _lastRunDate = BusinessClock.TodayDateOnly();
                }

                var delayHours = Math.Max(1, _options.RepeatEveryHoursWhenMissed);
                await Task.Delay(TimeSpan.FromHours(delayHours), stoppingToken);
            }
        }

        private bool ShouldRunNow()
        {
            var now = BusinessClock.Now();
            var today = DateOnly.FromDateTime(now);
            if (_lastRunDate == today)
            {
                return false;
            }

            var runAt = now.Date.AddHours(Math.Clamp(_options.RunHour, 0, 23))
                .AddMinutes(Math.Clamp(_options.RunMinute, 0, 59));
            return now >= runAt;
        }

        private async Task SendTodayRemindersAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var push = scope.ServiceProvider.GetRequiredService<IPushNotificationServices>();
                var today = BusinessClock.Today();
                var tomorrow = today.AddDays(1);

                var appointments = await context.Appointments
                    .Where(appointment =>
                        !appointment.IsDeleted &&
                        appointment.UserId.HasValue &&
                        appointment.AppointmentDate >= today &&
                        appointment.AppointmentDate < tomorrow &&
                        appointment.Status != AppointmentStatus.Cancelled &&
                        appointment.Status != AppointmentStatus.Completed)
                    .Include(appointment => appointment.Doctor)
                    .Include(appointment => appointment.Clinic)
                    .ToListAsync(cancellationToken);

                foreach (var appointment in appointments)
                {
                    try
                    {
                        var marker = ReminderMarker(appointment.Id, today);
                        var alreadySent = await context.Notifications.AnyAsync(notification =>
                            notification.UserId == appointment.UserId &&
                            notification.Message.Contains(marker),
                            cancellationToken);

                        if (alreadySent)
                        {
                            continue;
                        }

                        var title = "تذكير بالحجز";
                        var body = $"لديك حجز اليوم عند {appointment.Doctor?.Name ?? "الطبيب"} في {appointment.Clinic?.Name ?? "العيادة"}. رقم الدور: {appointment.QueueNumber}";
                        var data = new Dictionary<string, string>
                        {
                            ["type"] = "appointment_reminder",
                            ["appointmentId"] = appointment.Id.ToString(),
                            ["appointmentDate"] = appointment.AppointmentDate.ToString("yyyy-MM-dd")
                        };
                        var sent = await push.SendToUserAsync(
                            appointment.UserId.Value,
                            title,
                            body,
                            data,
                            cancellationToken);
                        NotificationDeliveryAttemptRecorder.AddPushAttempt(
                            context,
                            sent,
                            appointment.UserId.Value,
                            title,
                            body,
                            data,
                            doctorId: appointment.DoctorId,
                            clinicId: appointment.ClinicId,
                            appointmentId: appointment.Id);

                        context.Notifications.Add(new Notification
                        {
                            UserId = appointment.UserId,
                            DoctorId = appointment.DoctorId,
                            Message = $"{marker} {body}",
                            CreatedAt = BusinessClock.Now(),
                            Status = NotificationStatus.Unread,
                            CreatorId = appointment.UserId
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to send reminder for appointment {AppointmentId}.", appointment.Id);
                    }
                }

                if (appointments.Count > 0)
                {
                    await context.SaveChangesAsync(cancellationToken);
                }

                _logger.LogInformation("Appointment reminders checked. Count={Count}, Date={Date}", appointments.Count, today);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AppointmentReminderService failed entirely.");
            }
        }

        private static string ReminderMarker(int appointmentId, DateTime date) =>
            $"[appointment-reminder:{appointmentId}:{date:yyyy-MM-dd}]";
    }
}
