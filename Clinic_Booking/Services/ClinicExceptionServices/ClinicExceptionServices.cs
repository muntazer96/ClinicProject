using Clinic_Booking.Data;
using Clinic_Booking.DTOs.AppointmentDTO;
using Clinic_Booking.DTOs.ClinicExceptionDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.AuditLog;
using Clinic_Booking.Entities.ClinicException;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.IAppointmentReschedulingServices;
using Clinic_Booking.IServices.IClinicExceptionServices;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.INotificationDeliveryHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic_Booking.Utilities;

namespace Clinic_Booking.Services.ClinicExceptionServices
{
    public class ClinicExceptionServices : IClinicExceptionServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;
        private readonly IAppointmentReschedulingServices _appointmentReschedulingServices;
        private readonly INotificationDeliveryHelper _notificationHelper;

        public ClinicExceptionServices(
            ApplicationDbContext context,
            ILoadServices load,
            IAppointmentReschedulingServices appointmentReschedulingServices,
            INotificationDeliveryHelper notificationHelper)
        {
            _context = context;
            _load = load;
            _appointmentReschedulingServices = appointmentReschedulingServices;
            _notificationHelper = notificationHelper;
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
                return NotFound("العيادة غير موجودة أو لا تملك صلاحية إدارتها.");
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

            return Ok(exceptions, "تم جلب أيام الاستثناء بنجاح.");
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
                return NotFound("العيادة غير موجودة أو لا تملك صلاحية إدارتها.");
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
                return NotFound("يوم الاستثناء غير موجود.");
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
                    AddAuditLog(
                        form.IsClosed ? "ClinicExceptionClosed" : "ClinicExceptionUpdated",
                        "ClinicException",
                        exception.Id.ToString(),
                        doctorId.Value,
                        exception.ClinicId,
                        null,
                        null,
                        form.IsClosed
                            ? $"Closed exception date {exceptionDate:yyyy-MM-dd}; affected appointments: {pendingNotifications.Count}; action: {NormalizeConflictAction(form.AppointmentConflictAction) ?? "none"}."
                            : $"Updated exception date {exceptionDate:yyyy-MM-dd}.");
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateException)
                {
                    return BadRequest("لا يمكن إضافة أكثر من يوم استثناء لنفس العيادة في نفس التاريخ.");
                }
            }

            await _notificationHelper.SendPendingNotificationsAsync(pendingNotifications);

            return Ok(new { exception.Id }, "تم حفظ يوم الاستثناء بنجاح.");
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
                return NotFound("يوم الاستثناء غير موجود أو لا تملك صلاحية حذفه.");
            }

            exception.IsDeleted = true;
            exception.DeleterId = _load.GetCurrentUserId();
            exception.DeletedAt = DateTime.UtcNow;
            AddAuditLog(
                "ClinicExceptionDeleted",
                "ClinicException",
                exception.Id.ToString(),
                doctorId.Value,
                exception.ClinicId,
                null,
                null,
                $"Deleted exception date {exception.ExceptionDate:yyyy-MM-dd}.");
            await _context.SaveChangesAsync();

            return Ok<object>(null, "تم حذف يوم الاستثناء بنجاح.");
        }

        private async Task<IActionResult?> ValidateAsync(UpsertClinicExceptionDto form, int doctorId)
        {
            if (form.ClinicId <= 0 || form.ExceptionDate == default)
            {
                return BadRequest("العيادة وتاريخ الاستثناء مطلوبان.");
            }

            if (form.ExceptionDate < BusinessClock.TodayDateOnly())
            {
                return BadRequest("لا يمكن إضافة يوم استثناء بتاريخ سابق.");
            }

            if (form.ClosureReason?.Length > 500)
            {
                return BadRequest("سبب الإغلاق يجب أن لا يتجاوز 500 حرف.");
            }

            if (form.MaxAppointments is < 0)
            {
                return BadRequest("الحد الأقصى للحجوزات لا يمكن أن يكون سالباً.");
            }

            if (form.StartTime.HasValue != form.EndTime.HasValue ||
                form.StartTime.HasValue && form.StartTime >= form.EndTime)
            {
                return BadRequest("يجب إدخال وقت البداية والنهاية معاً، ويجب أن يكون وقت النهاية بعد وقت البداية.");
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
                return BadRequest("يجب وجود اشتراك نشط لإدارة أيام الاستثناء.");
            }

            if (form.MaxAppointments > packageLimit.Value)
            {
                return BadRequest($"الحد الأقصى للحجوزات لا يمكن أن يتجاوز حد الباقة ({packageLimit}).");
            }

            var action = NormalizeConflictAction(form.AppointmentConflictAction);
            if (form.IsClosed && action == null)
            {
                var exceptionDate = form.ExceptionDate.ToDateTime(TimeOnly.MinValue);
                var hasConflictingAppointments = await _context.Appointments.AnyAsync(appointment =>
                    appointment.ClinicId == form.ClinicId &&
                    appointment.DoctorId == doctorId &&
                    appointment.AppointmentDate.Date == exceptionDate.Date &&
                    appointment.Status != AppointmentStatus.Cancelled &&
                    appointment.Status != AppointmentStatus.Completed &&
                    !appointment.IsDeleted);

                if (hasConflictingAppointments)
                {
                    return BadRequest("عند إغلاق يوم يحتوي حجوزات يجب اختيار نقل الحجوزات أو إلغائها.");
                }
            }

            if (action == "move")
            {
                if (!form.MoveAppointmentsToDate.HasValue)
                {
                    return BadRequest("يجب تحديد تاريخ جديد عند نقل الحجوزات.");
                }

                if (form.MoveAppointmentsToDate.Value <= form.ExceptionDate ||
                    form.MoveAppointmentsToDate.Value < BusinessClock.TodayDateOnly())
                {
                    return BadRequest("تاريخ النقل يجب أن يكون تاريخاً لاحقاً بعد تاريخ الاستثناء.");
                }

                var bookingWindowDays = await _context.Clinics
                    .Where(clinic => clinic.Id == form.ClinicId && !clinic.IsDeleted)
                    .Select(clinic => clinic.BookingWindowDays)
                    .FirstOrDefaultAsync();
                bookingWindowDays = bookingWindowDays <= 0 ? 7 : bookingWindowDays;
                var maxBookableDate = DateOnly.FromDateTime(BusinessClock.Today().AddDays(bookingWindowDays - 1));
                if (form.MoveAppointmentsToDate.Value > maxBookableDate)
                {
                    return BadRequest($"تاريخ النقل يجب أن يكون ضمن نافذة الحجز الخاصة بالعيادة ({bookingWindowDays} يوم).");
                }

                var moveDate = form.MoveAppointmentsToDate.Value.ToDateTime(TimeOnly.MinValue);
                var isMoveDateClosed = await _context.ClinicExceptions.AnyAsync(exception =>
                    exception.ClinicId == form.ClinicId &&
                    exception.ExceptionDate == moveDate &&
                    exception.IsClosed &&
                    !exception.IsDeleted);
                if (isMoveDateClosed)
                {
                    return BadRequest("تاريخ النقل المحدد مغلق لهذه العيادة.");
                }
            }
            else if (action != null && action != "cancel")
            {
                return BadRequest("إجراء معالجة الحجوزات غير مدعوم.");
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
                    form.MoveAppointmentsToDate.Value.ToDateTime(TimeOnly.MinValue),
                    form.ClosureReason);
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
                    ? "تم إغلاق العيادة في هذا اليوم."
                    : reason.Trim();
                appointment.CancelledAt = now;
                appointment.CancelledByUserId = _load.GetCurrentUserId();
                appointment.ModifiedAt = now;
                appointment.ModifierId = _load.GetCurrentUserId();
                notifications.Add(ToPendingNotification(
                    appointment,
                    "تم إلغاء الحجز",
                    $"تم إلغاء حجزك بتاريخ {appointment.AppointmentDate:yyyy/MM/dd}. السبب: {appointment.CancellationReason}"));
            }

            return notifications;
        }

        private async Task<List<PendingAppointmentNotification>> MoveConflictingAppointmentsAsync(
            List<Entities.Appointment.Appointment> appointments,
            DateTime targetDate,
            string? cancellationReason)
        {
            var clinicId = appointments[0].ClinicId;
            var moveStates = new Dictionary<DateTime, AppointmentMoveCapacityState>();

            var notifications = new List<PendingAppointmentNotification>();
            foreach (var appointment in appointments)
            {
                var moveTarget = await _appointmentReschedulingServices.FindNextMoveTargetAsync(
                    clinicId,
                    targetDate.Date,
                    moveStates);

                if (moveTarget == null)
                {
                    notifications.Add(CancelAppointment(
                        appointment,
                        string.IsNullOrWhiteSpace(cancellationReason)
                            ? "لا توجد مواعيد متاحة لنقل الحجز بعد إغلاق العيادة."
                            : cancellationReason.Trim()));
                    continue;
                }

                ApplyMove(appointment, moveTarget);
                notifications.Add(ToPendingNotification(
                    appointment,
                    "تم تغيير موعد الحجز",
                    $"تم نقل حجزك إلى {appointment.AppointmentDate:yyyy/MM/dd}. رقم الدور الجديد #{appointment.QueueNumber}."));
            }

            return notifications;
        }

        private PendingAppointmentNotification CancelAppointment(
            Entities.Appointment.Appointment appointment,
            string reason)
        {
            var now = DateTime.UtcNow;
            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancellationReason = reason;
            appointment.CancelledAt = now;
            appointment.CancelledByUserId = _load.GetCurrentUserId();
            appointment.ModifiedAt = now;
            appointment.ModifierId = _load.GetCurrentUserId();

            return ToPendingNotification(
                appointment,
                "تم إلغاء الحجز",
                $"تم إلغاء حجزك بتاريخ {appointment.AppointmentDate:yyyy/MM/dd}. السبب: {appointment.CancellationReason}");
        }

        private void ApplyMove(Entities.Appointment.Appointment appointment, AppointmentMoveTargetDto moveTarget)
        {
            appointment.AppointmentDate = moveTarget.Date;
            appointment.QueueNumber = moveTarget.QueueNumber;
            appointment.ModifiedAt = DateTime.UtcNow;
            appointment.ModifierId = _load.GetCurrentUserId();
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
                appointment.GuestPhoneNumber,
                appointment.Status,
                title,
                body);
        }

        private void AddAuditLog(
            string action,
            string entityType,
            string? entityId,
            int? doctorId,
            int? clinicId,
            int? appointmentId,
            int? subscriptionId,
            string? details)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                UserId = _load.GetCurrentUserId(),
                DoctorId = doctorId,
                ClinicId = clinicId,
                AppointmentId = appointmentId,
                SubscriptionId = subscriptionId,
                Details = details,
                OccurredAt = DateTime.UtcNow
            });
        }

        private static string? NormalizeConflictAction(string? action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                return null;
            }

            var normalized = action.Trim().ToLowerInvariant();
            return normalized == "none" ? null : normalized;
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
                Message = "يجب تسجيل الدخول بحساب طبيب مرتبط."
            });
        }

    }
}
