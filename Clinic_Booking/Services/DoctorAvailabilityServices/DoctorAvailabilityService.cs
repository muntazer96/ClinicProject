using Clinic_Booking.Data;
using Clinic_Booking.DTOs.DoctorAvailabilityDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.DoctorAvailability;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.IDoctorAvailabilityServices;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.IPushNotificationServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.DoctorAvailabilityServices
{
    public class DoctorAvailabilityService : IDoctorAvailabilityServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;
        private readonly IPushNotificationServices _pushNotificationServices;
        public DoctorAvailabilityService(
            ApplicationDbContext context,
            ILoadServices load,
            IPushNotificationServices pushNotificationServices)
        {
            _context = context;
            _load = load;
            _pushNotificationServices = pushNotificationServices;
        }
        //public async Task<IActionResult> SetWeeklyAvailabilityAsync(AddDoctorAvailabilityDto dto)
        //{
        //    var doctor = await _context.Doctors
        //        .Include(d => d.DoctorSubscriptions)
        //        .Where(d => d.Id == dto.DoctorId && !d.IsDeleted)
        //        .FirstOrDefaultAsync();

        //    if (doctor == null)
        //    {
        //        return new NotFoundObjectResult(new ResponseDto<object>
        //        {
        //            Status = "Error",
        //            Code = 404,
        //            Message = "الدكتور غير موجود.",
        //            Data = null
        //        });
        //    }

        //    var now = DateTime.UtcNow;

        //    // Get current active subscription
        //    var activeSub = await _context.DoctorSubscriptions
        //        .Include(ds => ds.Package)
        //        .Where(ds => ds.DoctorId == dto.DoctorId && ds.StartDate <= now && ds.EndDate >= now)
        //        .OrderByDescending(ds => ds.StartDate)
        //        .FirstOrDefaultAsync();

        //    if (activeSub == null)
        //    {
        //        return new BadRequestObjectResult(new ResponseDto<object>
        //        {
        //            Status = "Error",
        //            Code = 400,
        //            Message = "لا يوجد اشتراك نشط للطبيب.",
        //            Data = null
        //        });
        //    }

        //    var package = activeSub.Package;

        //    if (dto.Days.Count > package.MaxWeeklyDays)
        //    {
        //        return new BadRequestObjectResult(new ResponseDto<object>
        //        {
        //            Status = "Error",
        //            Code = 400,
        //            Message = $"عدد الأيام المحددة أكبر من الحد المسموح به ({package.MaxWeeklyDays}) في هذه الباقة.",
        //            Data = null
        //        });
        //    }

        //    if (dto.Days.GroupBy(d => d.DayId).Any(g => g.Count() > 1))
        //    {
        //        return new BadRequestObjectResult(new ResponseDto<object>
        //        {
        //            Status = "Error",
        //            Code = 400,
        //            Message = "لا يمكن تكرار نفس اليوم أكثر من مرة.",
        //            Data = null
        //        });
        //    }

        //    foreach (var day in dto.Days)
        //    {
        //        if (day.MaxAppointments > package.MaxDailyAppointments)
        //        {
        //            return new BadRequestObjectResult(new ResponseDto<object>
        //            {
        //                Status = "Error",
        //                Code = 400,
        //                Message = $"عدد الحجوزات في اليوم (يوم رقم {day.DayId}) تجاوز الحد الأقصى المسموح به ({package.MaxDailyAppointments}).",
        //                Data = null
        //            });
        //        }
        //    }

        //    // Remove previous availabilities
        //    var oldAvailabilities = await _context.DoctorAvailabilities
        //        .Where(a => a.DoctorId == dto.DoctorId)
        //        .ToListAsync();

        //    _context.DoctorAvailabilities.RemoveRange(oldAvailabilities);

        //    // Add new availability records
        //    var newAvailabilities = dto.Days.Select(d => new DoctorAvailability
        //    {
        //        DoctorId = dto.DoctorId,
        //        DayId = d.DayId,
        //        StartTime = d.StartTime,
        //        EndTime = d.EndTime,
        //        MaxAppointments = d.MaxAppointments,
        //        IsAvailable = true
        //    }).ToList();

        //    _context.DoctorAvailabilities.AddRange(newAvailabilities);
        //    await _context.SaveChangesAsync();

        //    return new OkObjectResult(new ResponseDto<object>
        //    {
        //        Status = "Success",
        //        Code = 200,
        //        Message = "تم حفظ جدول التواجد الأسبوعي بنجاح.",
        //        Data = null
        //    });
        //}
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

            var now = DateTime.UtcNow;

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

            if (dto.Days.Count > package.MaxWeeklyDays)
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

            // حذف الأيام التي لم تعد موجودة
            var toDelete = existingAvailabilities.Where(a => !incomingDayIds.Contains(a.DayId)).ToList();
            _context.DoctorAvailabilities.RemoveRange(toDelete);

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
                    existing.ModifiedAt = DateTime.UtcNow;
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
                    Message = "حدث خطأ غير متوقع!",
                    Data = ex.Message
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

            var now = DateTime.UtcNow;

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
                availability.ModifiedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            if (!availability.IsAvailable)
            {
                await MoveAppointmentsFromDisabledDayAsync(
                    availability.ClinicId,
                    clinic.DoctorId,
                    day.NormalizedName);
            }

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

        private async Task MoveAppointmentsFromDisabledDayAsync(
            int clinicId,
            int doctorId,
            string disabledDayName)
        {
            var today = DateTime.Today;
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
                return;
            }

            var availableDays = await _context.DoctorAvailabilities
                .Include(availability => availability.Day)
                .Where(availability =>
                    availability.ClinicId == clinicId &&
                    availability.IsAvailable &&
                    !availability.IsDeleted)
                .ToListAsync();

            if (availableDays.Count == 0)
            {
                return;
            }

            foreach (var appointment in appointments)
            {
                var targetDate = FindNextAvailableDate(
                    appointment.AppointmentDate.Date.AddDays(1),
                    availableDays);
                if (!targetDate.HasValue)
                {
                    continue;
                }

                var nextQueueNumber = await _context.Appointments
                    .Where(item =>
                        item.ClinicId == clinicId &&
                        item.AppointmentDate.Date == targetDate.Value.Date &&
                        !item.IsDeleted)
                    .Select(item => (int?)item.QueueNumber)
                    .MaxAsync() ?? 0;

                appointment.AppointmentDate = targetDate.Value;
                appointment.QueueNumber = nextQueueNumber + 1;
                appointment.ModifiedAt = DateTime.UtcNow;
                appointment.ModifierId = _load.GetCurrentUserId();
            }

            await _context.SaveChangesAsync();

            foreach (var appointment in appointments)
            {
                await NotifyPatientAsync(
                    appointment,
                    "تم تغيير موعد الحجز",
                    $"تم نقل حجزك إلى {appointment.AppointmentDate:yyyy/MM/dd}، رقم الدور الجديد #{appointment.QueueNumber}.");
            }
        }

        private async Task NotifyPatientAsync(
            Entities.Appointment.Appointment appointment,
            string title,
            string body)
        {
            _context.Notifications.Add(new Entities.Notification.Notification
            {
                DoctorId = appointment.DoctorId,
                Message = body,
                CreatedAt = DateTime.UtcNow,
                Status = NotificationStatus.Unread
            });
            await _context.SaveChangesAsync();

            if (!appointment.UserId.HasValue)
            {
                return;
            }

            await _pushNotificationServices.SendToUserAsync(
                appointment.UserId.Value,
                title,
                body,
                new Dictionary<string, string>
                {
                    ["type"] = "booking",
                    ["appointmentId"] = appointment.Id.ToString(),
                    ["doctorId"] = appointment.DoctorId.ToString(),
                    ["clinicId"] = appointment.ClinicId.ToString(),
                    ["status"] = appointment.Status.ToString()
                });
        }

        private static DateTime? FindNextAvailableDate(
            DateTime startDate,
            List<DoctorAvailability> availableDays)
        {
            for (var offset = 0; offset < 31; offset++)
            {
                var candidate = startDate.AddDays(offset);
                if (availableDays.Any(availability =>
                        availability.Day.NormalizedName == candidate.DayOfWeek.ToString()))
                {
                    return candidate.Date;
                }
            }

            return null;
        }

    }
}
