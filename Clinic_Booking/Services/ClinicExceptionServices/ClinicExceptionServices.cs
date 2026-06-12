using Clinic_Booking.Data;
using Clinic_Booking.DTOs.ClinicExceptionDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.ClinicException;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.IClinicExceptionServices;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.IPushNotificationServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.ClinicExceptionServices
{
    public class ClinicExceptionServices : IClinicExceptionServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;
        private readonly IPushNotificationServices _pushNotificationServices;

        public ClinicExceptionServices(
            ApplicationDbContext context,
            ILoadServices load,
            IPushNotificationServices pushNotificationServices)
        {
            _context = context;
            _load = load;
            _pushNotificationServices = pushNotificationServices;
        }

        public async Task<IActionResult> GetMineAsync(int clinicId, DateOnly? fromDate, DateOnly? toDate)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue)
            {
                return Unauthorized();
            }

            if (!await OwnsClinicAsync(clinicId, doctorId.Value))
            {
                return NotFound("Clinic not found or you do not have permission to manage it.");
            }

            var query = _context.ClinicExceptions
                .Where(exception => exception.ClinicId == clinicId && !exception.IsDeleted);

            if (fromDate.HasValue)
            {
                query = query.Where(exception => exception.ExceptionDate >= fromDate.Value.ToDateTime(TimeOnly.MinValue));
            }

            if (toDate.HasValue)
            {
                query = query.Where(exception => exception.ExceptionDate <= toDate.Value.ToDateTime(TimeOnly.MinValue));
            }

            var exceptions = await query
                .OrderBy(exception => exception.ExceptionDate)
                .Select(exception => new GetClinicExceptionDto
                {
                    Id = exception.Id,
                    ClinicId = exception.ClinicId,
                    ExceptionDate = DateOnly.FromDateTime(exception.ExceptionDate),
                    IsClosed = exception.IsClosed,
                    ClosureReason = exception.ClosureReason,
                    MaxAppointments = exception.MaxAppointments,
                    StartTime = exception.StartTime,
                    EndTime = exception.EndTime
                })
                .ToListAsync();

            return Ok(exceptions, "Clinic exceptions retrieved successfully.");
        }

        public async Task<IActionResult> UpsertMineAsync(UpsertClinicExceptionDto form)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue)
            {
                return Unauthorized();
            }

            if (!await OwnsClinicAsync(form.ClinicId, doctorId.Value))
            {
                return NotFound("Clinic not found or you do not have permission to manage it.");
            }

            var validation = await ValidateAsync(form, doctorId.Value);
            if (validation != null)
            {
                return validation;
            }

            var exceptionDate = form.ExceptionDate.ToDateTime(TimeOnly.MinValue);
            var exception = form.Id.HasValue
                ? await _context.ClinicExceptions.FirstOrDefaultAsync(item =>
                    item.Id == form.Id &&
                    item.ClinicId == form.ClinicId &&
                    !item.IsDeleted)
                : await _context.ClinicExceptions.FirstOrDefaultAsync(item =>
                    item.ClinicId == form.ClinicId &&
                    item.ExceptionDate == exceptionDate &&
                    !item.IsDeleted);

            if (form.Id.HasValue && exception == null)
            {
                return NotFound("Clinic exception not found.");
            }

            if (exception == null)
            {
                exception = new ClinicException
                {
                    ClinicId = form.ClinicId,
                    ExceptionDate = exceptionDate,
                    CreatorId = _load.GetCurrentUserId()
                };
                _context.ClinicExceptions.Add(exception);
            }
            else
            {
                exception.ModifierId = _load.GetCurrentUserId();
                exception.ModifiedAt = DateTime.UtcNow;
            }

            exception.ExceptionDate = exceptionDate;
            exception.IsClosed = form.IsClosed;
            exception.ClosureReason = form.ClosureReason?.Trim();
            exception.MaxAppointments = form.MaxAppointments;
            exception.StartTime = form.StartTime;
            exception.EndTime = form.EndTime;

            List<PendingAppointmentNotification> pendingNotifications;
            await using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.SaveChangesAsync();
                    pendingNotifications = await HandleConflictingAppointmentsAsync(form, doctorId.Value, exceptionDate);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateException)
                {
                    return BadRequest("Only one exception can be configured for a clinic on the same date.");
                }
            }

            await SendPendingNotificationsAsync(pendingNotifications);

            return Ok(new { exception.Id }, "Clinic exception saved successfully.");
        }

        public async Task<IActionResult> DeleteMineAsync(int id)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue)
            {
                return Unauthorized();
            }

            var exception = await _context.ClinicExceptions
                .Include(item => item.Clinic)
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.Clinic.DoctorId == doctorId &&
                    !item.IsDeleted);

            if (exception == null)
            {
                return NotFound("Clinic exception not found or you do not have permission to delete it.");
            }

            exception.IsDeleted = true;
            exception.DeleterId = _load.GetCurrentUserId();
            exception.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok<object>(null, "Clinic exception deleted successfully.");
        }

        private async Task<IActionResult?> ValidateAsync(UpsertClinicExceptionDto form, int doctorId)
        {
            if (form.ClinicId <= 0 || form.ExceptionDate == default)
            {
                return BadRequest("Clinic and exception date are required.");
            }

            if (form.ExceptionDate < DateOnly.FromDateTime(DateTime.Today))
            {
                return BadRequest("Clinic exceptions cannot be configured for a past date.");
            }

            if (form.ClosureReason?.Length > 500)
            {
                return BadRequest("Closure reason cannot exceed 500 characters.");
            }

            if (form.MaxAppointments is < 0)
            {
                return BadRequest("Maximum appointments cannot be negative.");
            }

            if (form.StartTime.HasValue != form.EndTime.HasValue ||
                form.StartTime.HasValue && form.StartTime >= form.EndTime)
            {
                return BadRequest("Display start and end times must be provided together, and the end time must be later.");
            }

            var now = DateTime.UtcNow;
            var packageLimit = await _context.DoctorSubscriptions
                .Where(subscription =>
                    subscription.DoctorId == doctorId &&
                    subscription.Status == SubscriptionStatus.Active &&
                    subscription.StartDate <= now &&
                    subscription.EndDate >= now)
                .OrderByDescending(subscription => subscription.StartDate)
                .Select(subscription => (int?)subscription.Package.MaxDailyAppointments)
                .FirstOrDefaultAsync();

            if (!packageLimit.HasValue)
            {
                return BadRequest("An active subscription is required to configure clinic exceptions.");
            }

            if (form.MaxAppointments > packageLimit.Value)
            {
                return BadRequest($"Maximum appointments cannot exceed your package limit ({packageLimit}).");
            }

            var action = NormalizeConflictAction(form.AppointmentConflictAction);
            if (action == "move")
            {
                if (!form.MoveAppointmentsToDate.HasValue)
                {
                    return BadRequest("A new date is required when moving bookings.");
                }

                if (form.MoveAppointmentsToDate.Value <= form.ExceptionDate ||
                    form.MoveAppointmentsToDate.Value < DateOnly.FromDateTime(DateTime.Today))
                {
                    return BadRequest("The new booking date must be a future date after the exception date.");
                }
            }
            else if (action != null && action != "cancel")
            {
                return BadRequest("Unsupported appointment conflict action.");
            }

            return null;
        }

        private async Task<List<PendingAppointmentNotification>> HandleConflictingAppointmentsAsync(
            UpsertClinicExceptionDto form,
            int doctorId,
            DateTime exceptionDate)
        {
            if (!form.IsClosed)
            {
                return [];
            }

            var action = NormalizeConflictAction(form.AppointmentConflictAction);
            if (action == null)
            {
                return [];
            }

            var appointments = await _context.Appointments
                .Where(appointment =>
                    appointment.ClinicId == form.ClinicId &&
                    appointment.DoctorId == doctorId &&
                    appointment.AppointmentDate.Date == exceptionDate.Date &&
                    appointment.Status != AppointmentStatus.Cancelled &&
                    appointment.Status != AppointmentStatus.Completed &&
                    !appointment.IsDeleted)
                .OrderBy(appointment => appointment.QueueNumber)
                .ToListAsync();

            if (appointments.Count == 0)
            {
                return [];
            }

            if (action == "cancel")
            {
                return CancelConflictingAppointments(appointments, form.ClosureReason);
            }

            if (action == "move" && form.MoveAppointmentsToDate.HasValue)
            {
                return await MoveConflictingAppointmentsAsync(
                    appointments,
                    form.MoveAppointmentsToDate.Value.ToDateTime(TimeOnly.MinValue));
            }

            return [];
        }

        private List<PendingAppointmentNotification> CancelConflictingAppointments(
            List<Entities.Appointment.Appointment> appointments,
            string? reason)
        {
            var now = DateTime.UtcNow;
            var notifications = new List<PendingAppointmentNotification>();
            foreach (var appointment in appointments)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                appointment.CancellationReason = string.IsNullOrWhiteSpace(reason)
                    ? "Clinic exception."
                    : reason.Trim();
                appointment.CancelledAt = now;
                appointment.CancelledByUserId = _load.GetCurrentUserId();
                appointment.ModifiedAt = now;
                appointment.ModifierId = _load.GetCurrentUserId();
                notifications.Add(ToPendingNotification(
                    appointment,
                    "Booking cancelled",
                    $"Your booking on {appointment.AppointmentDate:yyyy/MM/dd} was cancelled. Reason: {appointment.CancellationReason}"));
            }

            return notifications;
        }

        private async Task<List<PendingAppointmentNotification>> MoveConflictingAppointmentsAsync(
            List<Entities.Appointment.Appointment> appointments,
            DateTime targetDate)
        {
            var nextQueueNumber = await _context.Appointments
                .Where(appointment =>
                    appointment.ClinicId == appointments[0].ClinicId &&
                    appointment.AppointmentDate.Date == targetDate.Date &&
                    !appointment.IsDeleted)
                .Select(appointment => (int?)appointment.QueueNumber)
                .MaxAsync() ?? 0;

            var notifications = new List<PendingAppointmentNotification>();
            foreach (var appointment in appointments)
            {
                appointment.AppointmentDate = targetDate;
                appointment.QueueNumber = ++nextQueueNumber;
                appointment.ModifiedAt = DateTime.UtcNow;
                appointment.ModifierId = _load.GetCurrentUserId();
                notifications.Add(ToPendingNotification(
                    appointment,
                    "Booking rescheduled",
                    $"Your booking was moved to {appointment.AppointmentDate:yyyy/MM/dd}. New queue number #{appointment.QueueNumber}."));
            }

            return notifications;
        }

        private static PendingAppointmentNotification ToPendingNotification(
            Entities.Appointment.Appointment appointment,
            string title,
            string body)
        {
            return new PendingAppointmentNotification(
                appointment.DoctorId,
                appointment.ClinicId,
                appointment.Id,
                appointment.UserId,
                appointment.Status,
                title,
                body);
        }

        private async Task SendPendingNotificationsAsync(List<PendingAppointmentNotification> notifications)
        {
            if (notifications.Count == 0)
            {
                return;
            }

            var now = DateTime.UtcNow;
            _context.Notifications.AddRange(notifications.Select(notification => new Entities.Notification.Notification
            {
                DoctorId = notification.DoctorId,
                Message = notification.Body,
                CreatedAt = now,
                Status = NotificationStatus.Unread
            }));
            await _context.SaveChangesAsync();

            foreach (var notification in notifications.Where(item => item.UserId.HasValue))
            {
                await _pushNotificationServices.SendToUserAsync(
                    notification.UserId!.Value,
                    notification.Title,
                    notification.Body,
                    new Dictionary<string, string>
                    {
                        ["type"] = "booking",
                        ["appointmentId"] = notification.AppointmentId.ToString(),
                        ["doctorId"] = notification.DoctorId.ToString(),
                        ["clinicId"] = notification.ClinicId.ToString(),
                        ["status"] = notification.Status.ToString()
                    });
            }
        }

        private static string? NormalizeConflictAction(string? action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                return null;
            }

            return action.Trim().ToLowerInvariant();
        }

        private async Task<int?> GetCurrentDoctorIdAsync()
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return null;
            }

            return await _context.Doctors
                .Where(doctor => doctor.UserId == userId && !doctor.IsDeleted)
                .Select(doctor => (int?)doctor.Id)
                .FirstOrDefaultAsync();
        }

        private Task<bool> OwnsClinicAsync(int clinicId, int doctorId)
        {
            return _context.Clinics.AnyAsync(clinic =>
                clinic.Id == clinicId &&
                clinic.DoctorId == doctorId &&
                !clinic.IsDeleted);
        }

        private static IActionResult Ok<T>(T data, string message)
        {
            return new OkObjectResult(new ResponseDto<T>
            {
                Status = "Success",
                Code = 200,
                Message = message,
                Data = data
            });
        }

        private static IActionResult BadRequest(string message)
        {
            return new BadRequestObjectResult(new ResponseDto<string>
            {
                Status = "Error",
                Code = 400,
                Message = message
            });
        }

        private static IActionResult NotFound(string message)
        {
            return new NotFoundObjectResult(new ResponseDto<string>
            {
                Status = "Error",
                Code = 404,
                Message = message
            });
        }

        private static IActionResult Unauthorized()
        {
            return new UnauthorizedObjectResult(new ResponseDto<string>
            {
                Status = "Error",
                Code = 401,
                Message = "You must sign in with a linked doctor account."
            });
        }

        private sealed record PendingAppointmentNotification(
            int DoctorId,
            int ClinicId,
            int AppointmentId,
            Guid? UserId,
            AppointmentStatus Status,
            string Title,
            string Body);
    }
}
