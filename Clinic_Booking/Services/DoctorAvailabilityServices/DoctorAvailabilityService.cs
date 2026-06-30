using Clinic_Booking.Data;
using Clinic_Booking.DTOs.DoctorAvailabilityDTO;
using Clinic_Booking.DTOs.AppointmentDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.AuditLog;
using Clinic_Booking.Entities.DoctorAvailability;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.IAppointmentReschedulingServices;
using Clinic_Booking.IServices.IDoctorAvailabilityServices;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.INotificationDeliveryHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic_Booking.Utilities;

namespace Clinic_Booking.Services.DoctorAvailabilityServices
{
    public class DoctorAvailabilityService : IDoctorAvailabilityServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;
        private readonly IAppointmentReschedulingServices _appointmentReschedulingServices;
        private readonly INotificationDeliveryHelper _notificationHelper;
        public DoctorAvailabilityService(
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
        public async Task<IActionResult> UpsertWeeklyAvailabilityAsync(AddDoctorAvailabilityDto dto)
        {
            var clinic = await _context.Clinics
                .Include(c => c.Doctor)
                .Where(c => c.Id == dto.ClinicId && !c.IsDeleted && !c.Doctor.IsDeleted)
                .FirstOrDefaultAsync();

            if (clinic == null)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "العيادة غير موجودة.",
                    Data = null
                });
            }

            if (clinic.Doctor.UserId != _load.GetCurrentUserId())
            {
                return new UnauthorizedObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 401,
                    Message = "You do not have permission to manage this clinic.",
                    Data = null
                });
            }

            var now = BusinessClock.Now();

            var activeSub = await _context.DoctorSubscriptions
                .Include(ds => ds.Package)
                .Where(ds =>
                    ds.DoctorId == clinic.DoctorId &&
                    ds.Status == Clinic_Booking.Enums.SubscriptionStatus.Active &&
                    ds.StartDate <= now &&
                    ds.EndDate >= now)
                .OrderByDescending(ds => ds.StartDate)
                .FirstOrDefaultAsync();

            if (activeSub == null)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "لا يوجد اشتراك نشط للطبيب.",
                    Data = null
                });
            }

            var package = activeSub.Package;

            if (dto.Days == null)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "يجب تحديد أيام الدوام.",
                    Data = null
                });
            }

            var selectedWeeklyDayCount = await CountWeeklyDaysAfterBulkUpdateAsync(
                clinic.DoctorId,
                dto.ClinicId,
                dto.Days.Select(day => day.DayId));

            if (selectedWeeklyDayCount > package.MaxWeeklyDays)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = $"عدد الأيام المحددة أكبر من الحد المسموح به ({package.MaxWeeklyDays}) في هذه الباقة.",
                    Data = null
                });
            }

            if (dto.Days.GroupBy(d => d.DayId).Any(g => g.Count() > 1))
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "لا يمكن تكرار نفس اليوم أكثر من مرة.",
                    Data = null
                });
            }

            foreach (var day in dto.Days)
            {
                if (day.MaxAppointments > package.MaxDailyAppointments)
                {
                    return new BadRequestObjectResult(new ResponseDto<object>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = $"عدد الحجوزات في اليوم (يوم رقم {day.DayId}) تجاوز الحد الأقصى المسموح به ({package.MaxDailyAppointments}).",
                        Data = null
                    });
                }
            }

            var existingAvailabilities = await _context.DoctorAvailabilities
                .Where(a => a.ClinicId == dto.ClinicId)
                .ToListAsync();

            var incomingDayIds = dto.Days.Select(d => d.DayId).ToList();
            var toDisable = existingAvailabilities
                .Where(a => a.IsAvailable && !incomingDayIds.Contains(a.DayId))
                .ToList();
            var disabledDayNames = await _context.Days
                .Where(day => toDisable.Select(availability => availability.DayId).Contains(day.Id))
                .ToDictionaryAsync(day => day.Id, day => day.NormalizedName);

            List<PendingAppointmentNotification> pendingNotifications = [];
            await using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                foreach (var availability in toDisable)
                {
                    availability.IsAvailable = false;
                    availability.ModifierId = _load.GetCurrentUserId();
                    availability.ModifiedAt = BusinessClock.Now();
                }

                // تحديث أو إضافة الأيام الجديدة
                foreach (var day in dto.Days)
                {
                    var existing = existingAvailabilities.FirstOrDefault(a => a.DayId == day.DayId);
                    if (existing != null)
                    {
                        // تحديث
                        existing.StartTime = day.StartTime;
                        existing.EndTime = day.EndTime;
                        existing.MaxAppointments = day.MaxAppointments;
                        existing.IsAvailable = true;
                        existing.ModifierId = _load.GetCurrentUserId();
                        existing.ModifiedAt = BusinessClock.Now();
                    }
                    else
                    {
                        // إضافة
                        var newAvailability = new DoctorAvailability
                        {
                            ClinicId = dto.ClinicId,
                            DayId = day.DayId,
                            StartTime = day.StartTime,
                            EndTime = day.EndTime,
                            MaxAppointments = day.MaxAppointments,
                            IsAvailable = true,
                            CreatorId = _load.GetCurrentUserId(),
                        };
                        _context.DoctorAvailabilities.Add(newAvailability);
                    }
                }

                await _context.SaveChangesAsync();

                foreach (var availability in toDisable)
                {
                    if (!disabledDayNames.TryGetValue(availability.DayId, out var dayName))
                    {
                        continue;
                    }

                    pendingNotifications.AddRange(await MoveAppointmentsFromDisabledDayAsync(
                        availability.ClinicId,
                        clinic.DoctorId,
                        dayName));
                    AddAuditLog(
                        "WorkingDayDisabledFromWeeklySchedule",
                        "DoctorAvailability",
                        availability.Id.ToString(),
                        clinic.DoctorId,
                        availability.ClinicId,
                        null,
                        null,
                        $"Disabled weekly day {dayName}; affected appointments so far: {pendingNotifications.Count}.");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            await _notificationHelper.SendPendingNotificationsAsync(pendingNotifications);

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "تم حفظ/تحديث جدول التواجد الأسبوعي بنجاح.",
                Data = null
            });
        }
        public async Task<IActionResult> GetWeeklyAvailabilityAsync(int clinicId)
        {
            try
            {
                var clinic = await _context.Clinics
                .Where(c => c.Id == clinicId && !c.IsDeleted)
                .FirstOrDefaultAsync();

                if (clinic == null)
                {
                    return new NotFoundObjectResult(new ResponseDto<object>
                    {
                        Status = "Error",
                        Code = 404,
                        Message = "العيادة غير موجودة.",
                        Data = null
                    });
                }

                var days = await _context.Days
                    .Where(d => !d.IsDeleted)
                    .OrderBy(d => d.Id)
                    .ToListAsync();

                var availabilities = await _context.DoctorAvailabilities
                    .Where(a => a.ClinicId == clinicId && !a.IsDeleted)
                    .ToListAsync();

                var availabilityByDay = availabilities
                    .GroupBy(a => a.DayId)
                    .ToDictionary(group => group.Key, group => group.First());

                var result = days.Select(day =>
                {
                    availabilityByDay.TryGetValue(day.Id, out var availability);

                    return new GetDoctorDayAvailabilityDto
                    {
                        Id = availability?.Id,
                        ClinicId = clinicId,
                        DayId = day.Id,
                        DayOfWeek = day.Id - 1,
                        DayName = day.Name,
                        DayNormailzedName = day.NormalizedName,
                        IsAvailable = availability?.IsAvailable ?? false,
                        StartTime = availability?.StartTime,
                        EndTime = availability?.EndTime,
                        MaxAppointments = availability?.MaxAppointments
                    };
                }).ToList();

                return new OkObjectResult(new ResponseDto<List<GetDoctorDayAvailabilityDto>>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "تم جلب أوقات الدوام بنجاح.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "حدث خطأ غير متوقع، يرجى المحاولة لاحقاً.",
                    Data = null
                })
                {
                    StatusCode = 500
                };
            }
        }
        public async Task<IActionResult> UpdateSingleDayAvailabilityAsync(UpdateSingleDayAvailabilityDto dto)
        {
            var clinic = await _context.Clinics
                .Include(c => c.Doctor)
                .Where(c => c.Id == dto.ClinicId && !c.IsDeleted && !c.Doctor.IsDeleted)
                .FirstOrDefaultAsync();

            if (clinic == null)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "العيادة غير موجودة.",
                    Data = null
                });
            }

            if (clinic.Doctor.UserId != _load.GetCurrentUserId())
            {
                return new UnauthorizedObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 401,
                    Message = "You do not have permission to manage this clinic.",
                    Data = null
                });
            }

            var now = BusinessClock.Now();

            var activeSub = await _context.DoctorSubscriptions
                .Include(ds => ds.Package)
                .Where(ds =>
                    ds.DoctorId == clinic.DoctorId &&
                    ds.Status == Clinic_Booking.Enums.SubscriptionStatus.Active &&
                    ds.StartDate <= now &&
                    ds.EndDate >= now)
                .OrderByDescending(ds => ds.StartDate)
                .FirstOrDefaultAsync();

            if (activeSub == null)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "لا يوجد اشتراك نشط للطبيب.",
                    Data = null
                });
            }

            var package = activeSub.Package;

            if (dto.MaxAppointments > package.MaxDailyAppointments)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = $"عدد الحجوزات لليوم (يوم رقم {dto.DayId}) تجاوز الحد الأقصى المسموح به ({package.MaxDailyAppointments}).",
                    Data = null
                });
            }

            var selectedWeeklyDayCount = await CountWeeklyDaysAfterSingleDayUpdateAsync(
                clinic.DoctorId,
                dto.ClinicId,
                dto.DayId,
                dto.IsAvailable);

            if (selectedWeeklyDayCount > package.MaxWeeklyDays)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = $"عدد الأيام المحددة أكبر من الحد المسموح به ({package.MaxWeeklyDays}) في هذه الباقة.",
                    Data = null
                });
            }

            var day = await _context.Days
                .Where(d => d.Id == dto.DayId && !d.IsDeleted)
                .FirstOrDefaultAsync();

            if (day == null)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "اليوم المحدد غير صحيح.",
                    Data = null
                });
            }

            var availability = await _context.DoctorAvailabilities
                .Where(a =>
                    a.ClinicId == dto.ClinicId &&
                    a.Clinic.DoctorId == clinic.DoctorId &&
                    a.DayId == dto.DayId &&
                    !a.IsDeleted)
                .FirstOrDefaultAsync();

            var isNewAvailability = false;

            if (availability == null)
            {
                isNewAvailability = true;
                availability = new DoctorAvailability
                {
                    ClinicId = dto.ClinicId,
                    DayId = dto.DayId,
                    CreatorId = _load.GetCurrentUserId(),
                };

                _context.DoctorAvailabilities.Add(availability);
            }

            availability.StartTime = dto.StartTime;
            availability.EndTime = dto.EndTime;
            availability.MaxAppointments = dto.MaxAppointments;
            availability.IsAvailable = dto.IsAvailable;

            if (!isNewAvailability)
            {
                availability.ModifierId = _load.GetCurrentUserId();
                availability.ModifiedAt = BusinessClock.Now();
            }

            List<PendingAppointmentNotification> pendingNotifications = [];
            await using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                await _context.SaveChangesAsync();

                if (!availability.IsAvailable)
                {
                    pendingNotifications = await MoveAppointmentsFromDisabledDayAsync(
                        availability.ClinicId,
                        clinic.DoctorId,
                        day.NormalizedName);
                    AddAuditLog(
                        "WorkingDayDisabled",
                        "DoctorAvailability",
                        availability.Id.ToString(),
                        clinic.DoctorId,
                        availability.ClinicId,
                        null,
                        null,
                        $"Disabled day {day.NormalizedName}; affected appointments: {pendingNotifications.Count}.");
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }

            await _notificationHelper.SendPendingNotificationsAsync(pendingNotifications);

            var result = new GetDoctorDayAvailabilityDto
            {
                Id = availability.Id,
                ClinicId = availability.ClinicId,
                DayId = availability.DayId,
                DayOfWeek = availability.DayId - 1,
                DayName = day.Name,
                DayNormailzedName = day.NormalizedName,
                IsAvailable = availability.IsAvailable,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                MaxAppointments = availability.MaxAppointments
            };

            return new OkObjectResult(new ResponseDto<GetDoctorDayAvailabilityDto>
            {
                Status = "Success",
                Code = 200,
                Message = isNewAvailability ? "تم إضافة معلومات اليوم بنجاح." : "تم تحديث معلومات اليوم بنجاح.",
                Data = result
            });
        }

        private async Task<List<PendingAppointmentNotification>> MoveAppointmentsFromDisabledDayAsync(
            int clinicId,
            int doctorId,
            string disabledDayName)
        {
            var today = BusinessClock.Today();
            var appointments = await _context.Appointments
                .Where(appointment =>
                    appointment.ClinicId == clinicId &&
                    appointment.DoctorId == doctorId &&
                    appointment.AppointmentDate.Date >= today &&
                    appointment.Status != AppointmentStatus.Cancelled &&
                    appointment.Status != AppointmentStatus.Completed &&
                    !appointment.IsDeleted)
                .OrderBy(appointment => appointment.AppointmentDate)
                .ThenBy(appointment => appointment.QueueNumber)
                .ToListAsync();

            appointments = appointments
                .Where(appointment =>
                    appointment.AppointmentDate.DayOfWeek.ToString() ==
                    disabledDayName)
                .ToList();

            if (appointments.Count == 0)
            {
                return [];
            }

            var moveStates = new Dictionary<DateTime, AppointmentMoveCapacityState>();
            var notifications = new List<PendingAppointmentNotification>();
            foreach (var appointment in appointments)
            {
                var moveTarget = await _appointmentReschedulingServices.FindNextMoveTargetAsync(
                    clinicId,
                    appointment.AppointmentDate.Date.AddDays(1),
                    moveStates);
                if (moveTarget == null)
                {
                    notifications.Add(CancelAppointment(
                        appointment,
                        "لا توجد مواعيد متاحة لنقل الحجز بعد تعطيل يوم الدوام."));
                    continue;
                }

                appointment.AppointmentDate = moveTarget.Date;
                appointment.QueueNumber = moveTarget.QueueNumber;
                appointment.ModifiedAt = BusinessClock.Now();
                appointment.ModifierId = _load.GetCurrentUserId();
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
            var now = BusinessClock.Now();
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

        private async Task<int> CountWeeklyDaysAfterBulkUpdateAsync(
            int doctorId,
            int clinicId,
            IEnumerable<int> incomingDayIds)
        {
            var activeDayIds = await _context.DoctorAvailabilities
                .Where(availability =>
                    availability.Clinic.DoctorId == doctorId &&
                    availability.ClinicId != clinicId &&
                    availability.IsAvailable &&
                    !availability.IsDeleted &&
                    !availability.Clinic.IsDeleted)
                .Select(availability => availability.DayId)
                .ToListAsync();

            activeDayIds.AddRange(incomingDayIds);
            return activeDayIds.Distinct().Count();
        }

        private async Task<int> CountWeeklyDaysAfterSingleDayUpdateAsync(
            int doctorId,
            int clinicId,
            int dayId,
            bool isAvailable)
        {
            var activeDayIds = await _context.DoctorAvailabilities
                .Where(availability =>
                    availability.Clinic.DoctorId == doctorId &&
                    !(availability.ClinicId == clinicId &&
                      availability.DayId == dayId) &&
                    availability.IsAvailable &&
                    !availability.IsDeleted &&
                    !availability.Clinic.IsDeleted)
                .Select(availability => availability.DayId)
                .ToListAsync();

            if (isAvailable)
            {
                activeDayIds.Add(dayId);
            }

            return activeDayIds.Distinct().Count();
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
                OccurredAt = BusinessClock.Now()
            });
        }

    }
}
