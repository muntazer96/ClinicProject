using Clinic_Booking.Data;
using Clinic_Booking.DTOs.AppointmentDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.Appointment;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.IAppointmentServices;
using Clinic_Booking.IServices.ILoadServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.AppointmentServices
{
    public class AppointmentServices : IAppointmentServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;
        public AppointmentServices(ApplicationDbContext context, ILoadServices load) 
        { 
            _context = context;
            _load = load;
        }
        public async Task<IActionResult> GetAppointmentsAsync(SearchAppointmentDto form)
        {
            var query = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.User)
                .AsQueryable();

            if (form.DoctorId.HasValue)
                query = query.Where(a => a.DoctorId == form.DoctorId.Value);

            if (form.UserId.HasValue)
                query = query.Where(a => a.UserId == form.UserId.Value);

            if(form.Status.HasValue)
                query = query.Where(a=> a.Status == form.Status.Value);

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new
                {
                    a.Id,
                    a.UserId,
                    Name = a.User.Name, // أو أي خاصية مناسبة للمستخدم
                    PhoneNumer = a.User.PhoneNumber,
                    a.DoctorId,
                    DoctorName = a.Doctor.Name, // افترض ان Doctor عنده خاصية Name
                    a.AppointmentDate,
                    Status = a.Status.ToString(),
                    PaymentAmount = a.PaymentAmount,
                    PaymentStatus = a.PaymentStatus.ToString()
                })
                .ToListAsync();

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "تم جلب الحجوزات بنجاح.",
                Data = appointments
            });
        }
        public async Task<IActionResult> CreateAppointmentAsync(AddAppointmentDto form)
        {
            var doctor = await _context.Doctors
                .Where(d => d.Id == form.DoctorId && !d.IsDeleted)
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

            // التحقق من خاصية الحجز الالكتروني (EBooking)
            var eBookingFeature = await _context.DoctorFeature
                .Include(df => df.Feature)
                .Where(df => df.DoctorId == form.DoctorId
                             && df.Feature.NormalizedName == "EBooking"
                             && df.IsEnabled)
                .FirstOrDefaultAsync();

            if (eBookingFeature == null)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "الحجز الإلكتروني غير مفعّل لهذا الطبيب.",
                    Data = null
                });
            }

            // اسم اليوم الموحد (يُفضل تحويله للـ UpperInvariant للتطابق مع NormalizedName)
            var dayName = form.AppointmentDate.DayOfWeek.ToString().ToUpperInvariant();

            var day = await _context.Days
                .Where(d => d.NormalizedName == dayName)
                .FirstOrDefaultAsync();

            if (day == null)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "اليوم المحدد للحجز غير معرف في النظام.",
                    Data = null
                });
            }

            var availability = await _context.DoctorAvailabilities
                .Where(a => a.DoctorId == form.DoctorId && a.DayId == day.Id && a.IsAvailable)
                .FirstOrDefaultAsync();

            if (availability == null)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "الطبيب غير متوفر في هذا اليوم.",
                    Data = null
                });
            }

            var existingAppointmentsCount = await _context.Appointments
                .Where(ap => ap.DoctorId == form.DoctorId
                        && ap.AppointmentDate.Date == form.AppointmentDate.Date
                        && ap.Status != AppointmentStatus.Cancelled)
                .CountAsync();

            if (existingAppointmentsCount >= availability.MaxAppointments)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "تم الوصول إلى الحد الأقصى للحجوزات لهذا اليوم لدى الطبيب.",
                    Data = null
                });
            }

            var userId = (Guid)_load.GetCurrentUserId();

            var userExistingAppointment = await _context.Appointments
                .Where(ap => ap.DoctorId == form.DoctorId
                          && ap.UserId == userId
                          && ap.AppointmentDate.Date == form.AppointmentDate.Date
                          && ap.Status != AppointmentStatus.Cancelled)
                .FirstOrDefaultAsync();

            if (userExistingAppointment != null)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "لديك حجز مسبق مع هذا الطبيب في نفس اليوم.",
                    Data = null
                });
            }

            var newAppointment = new Appointment
            {
                UserId = userId,
                DoctorId = form.DoctorId,
                AppointmentDate = form.AppointmentDate,
                Status = AppointmentStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                PaymentAmount = null
            };

            _context.Appointments.Add(newAppointment);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "تم إنشاء الحجز بنجاح.",
                Data = new { AppointmentId = newAppointment.Id }
            });
        }
        public async Task<IActionResult> ToggleAppointmentStatusAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "الحجز غير موجود.",
                    Data = null
                });
            }

            // Toggle logic:
            if(appointment.Status == AppointmentStatus.Completed)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = " لا يمكن تغيير حاله الحجز.",
                    Data = null
                });
            }
            else if (appointment.Status == AppointmentStatus.Confirmed)
            {
                appointment.Status = AppointmentStatus.Cancelled;
            }
            else if(appointment.Status == AppointmentStatus.Pending || appointment.Status == AppointmentStatus.Cancelled) 
            {
                appointment.Status = AppointmentStatus.Confirmed;
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = $"تم تحديث حالة الحجز إلى '{appointment.Status}'.",
                Data = null
            });
        }
        public async Task<IActionResult> CompleteAppointmentAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "الحجز غير موجود.",
                    Data = null
                });
            }

            if (appointment.Status != AppointmentStatus.Confirmed)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "لا يمكن إكمال الحجز إلا إذا كان في حالة مقبول.",
                    Data = null
                });
            }

            appointment.Status = AppointmentStatus.Completed;

            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "تم تحديث حالة الحجز إلى مكتمل.",
                Data = null
            });
        }

    }
}
