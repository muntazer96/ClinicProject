using Clinic_Booking.Data;
using Clinic_Booking.DTOs.DoctorAvailabilityDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.DoctorAvailability;
using Clinic_Booking.IServices.IDoctorAvailabilityServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.DoctorAvailabilityServices
{
    public class DoctorAvailabilityService : IDoctorAvailabilityServices
    {
        private readonly ApplicationDbContext _context;
        public DoctorAvailabilityService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> SetWeeklyAvailabilityAsync(AddDoctorAvailabilityDto dto)
        {
            var doctor = await _context.Doctors
                .Include(d => d.DoctorSubscriptions)
                .Where(d => d.Id == dto.DoctorId && !d.IsDeleted)
                .FirstOrDefaultAsync();

            if (doctor == null)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "الدكتور غير موجود.",
                    Data = null
                });
            }

            var now = DateTime.UtcNow;

            // Get current active subscription
            var activeSub = await _context.DoctorSubscriptions
                .Include(ds => ds.Package)
                .Where(ds => ds.DoctorId == dto.DoctorId && ds.StartDate <= now && ds.EndDate >= now)
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

            // Remove previous availabilities
            var oldAvailabilities = await _context.DoctorAvailabilities
                .Where(a => a.DoctorId == dto.DoctorId)
                .ToListAsync();

            _context.DoctorAvailabilities.RemoveRange(oldAvailabilities);

            // Add new availability records
            var newAvailabilities = dto.Days.Select(d => new DoctorAvailability
            {
                DoctorId = dto.DoctorId,
                DayId = d.DayId,
                StartTime = d.StartTime,
                EndTime = d.EndTime,
                MaxAppointments = d.MaxAppointments,
                IsAvailable = true
            }).ToList();

            _context.DoctorAvailabilities.AddRange(newAvailabilities);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "تم حفظ جدول التواجد الأسبوعي بنجاح.",
                Data = null
            });
        }

    }
}
