using Clinic_Booking.Data;
using Clinic_Booking.DTOs.DoctorAvailabilityDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.DoctorAvailability;
using Clinic_Booking.IServices.IDoctorAvailabilityServices;
using Clinic_Booking.IServices.ILoadServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.DoctorAvailabilityServices
{
    public class DoctorAvailabilityService : IDoctorAvailabilityServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;
        public DoctorAvailabilityService(ApplicationDbContext context,ILoadServices load)
        {
            _context = context;
            _load = load;
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

            var now = DateTime.UtcNow;

            var activeSub = await _context.DoctorSubscriptions
                .Include(ds => ds.Package)
                .Where(ds => ds.DoctorId == clinic.DoctorId && ds.StartDate <= now && ds.EndDate >= now)
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

                // اجلب التوفر الحالي
                var availabilities = await _context.DoctorAvailabilities
        .Where(a => a.ClinicId == clinicId)
        .Include(a => a.Day)
        .Select(availability => new GetDoctorDayAvailabilityDto
        {
            Id=availability.Id,
            ClinicId = availability.ClinicId,
            DayId = availability.DayId,
            DayName = availability.Day.Name,
            DayNormailzedName = availability.Day.NormalizedName,
            IsAvailable = availability.IsAvailable,
            StartTime = availability.StartTime,
            EndTime = availability.EndTime,
            MaxAppointments = availability.MaxAppointments
        })
        .ToListAsync();
                if(availabilities.Count == 0)
                {
                    return new NotFoundObjectResult(new ResponseDto<string>
                    {
                        Status = "Success",
                        Code = 403,
                        Message = "لا يتوفر جدول اسبوعي للعيادة.",
                    });
                }
                return new OkObjectResult(new ResponseDto<List<GetDoctorDayAvailabilityDto>>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "تم جلب حالة التوفر الأسبوعي بنجاح.",
                    Data = availabilities
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

            var now = DateTime.UtcNow;

            var activeSub = await _context.DoctorSubscriptions
                .Include(ds => ds.Package)
                .Where(ds => ds.DoctorId == clinic.DoctorId && ds.StartDate <= now && ds.EndDate >= now)
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

            var availability = await _context.DoctorAvailabilities
                .Where(a => a.ClinicId == dto.ClinicId && a.DayId == dto.DayId)
                .FirstOrDefaultAsync();

            if (availability == null)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "لم يتم العثور على توفر لهذا اليوم. الرجاء إضافته أولاً.",
                    Data = null
                });
            }

            // التحديث
            availability.StartTime = dto.StartTime;
            availability.EndTime = dto.EndTime;
            availability.MaxAppointments = dto.MaxAppointments;
            availability.IsAvailable = dto.IsAvailable;
            availability.ModifierId = _load.GetCurrentUserId();
            availability.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "تم تحديث معلومات اليوم بنجاح.",
                Data = null
            });
        }

    }
}
