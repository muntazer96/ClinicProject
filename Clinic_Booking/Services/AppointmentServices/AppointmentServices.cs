using Clinic_Booking.Data;
using Clinic_Booking.DTOs.AppointmentDTO;
using Clinic_Booking.DTOs.ClinicDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.Appointment;
using Clinic_Booking.Enums;
using Clinic_Booking.Extensions;
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

        public async Task<ActionResult<PaginationDto.PageResult<GetApponitmentDto>>> GetListAsync(
            SearchAppointmentDto form, int page = 1, int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "قيم الصفحة أو الحجم غير صحيحة.",
                    Data = null
                });
            }

            var query = _context.Appointments
                .Where(a => !a.IsDeleted)
                .Where(a => form.Id == null || a.Id == form.Id)
                .Where(a => form.DoctorId == null || a.DoctorId == form.DoctorId)
                .Where(a => form.ClinicId == null || a.ClinicId == form.ClinicId)
                .Where(a => form.UserId == null || a.UserId == form.UserId)
                .Where(a => form.FromDate == null || a.AppointmentDate.Date >= form.FromDate.Value.ToDateTime(TimeOnly.MinValue))
                .Where(a => form.ToDate == null || a.AppointmentDate.Date <= form.ToDate.Value.ToDateTime(TimeOnly.MinValue))
                .Where(a => form.Status == null || a.Status == form.Status);

            var totalItems = await query.CountAsync();
            if (totalItems == 0)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Not Found",
                    Code = 404,
                    Message = "لا توجد بيانات للعرض!",
                    Data = null
                });
            }

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.QueueNumber)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new GetApponitmentDto
                {
                    Id = a.Id,
                    User = a.UserId.HasValue
                        ? new DTOs.ReviewDTO.GetUserReivew
                        {
                            Id = a.UserId.Value,
                            Name = a.User.Name,
                            NormalizedName = a.User.NormalizedUserName
                        }
                        : null,
                    Doctor = new DTOs.ReviewDTO.GetDoctorReview
                    {
                        Id = a.Doctor.Id,
                        Name = a.Doctor.Name,
                        NormalizedName = a.Doctor.NormalizedName
                    },
                    Clinic = new GetClinicDto
                    {
                        Id = a.Clinic.Id,
                        DoctorId = a.Clinic.DoctorId,
                        Name = a.Clinic.Name,
                        IraqiProvince = a.Clinic.IraqiProvince,
                        IraqiProvinceName = a.Clinic.IraqiProvince.GetDisplayName(),
                        Address = a.Clinic.Address,
                        Latitude = a.Clinic.Latitude,
                        Longitude = a.Clinic.Longitude,
                        MapUrl = a.Clinic.MapUrl,
                        PhoneNumber = a.Clinic.PhoneNumber,
                        IsVisible = a.Clinic.IsVisible
                    },
                    AppointmentDate = a.AppointmentDate,
                    QueueNumber = a.QueueNumber,
                    GuestName = a.GuestName,
                    GuestPhoneNumber = a.GuestPhoneNumber,
                    IsPhoneConfirmed = a.IsPhoneConfirmed,
                    Status = a.Status,
                    PaymentStatus = a.PaymentStatus,
                    PaymentAmount = a.PaymentAmount
                })
                .ToListAsync();

            return new OkObjectResult(new ResponseDto<PaginationDto.PageResult<GetApponitmentDto>>
            {
                Status = "Success",
                Code = 200,
                Message = "تم جلب البيانات بنجاح!",
                Data = new PaginationDto.PageResult<GetApponitmentDto>(
                    appointments, totalItems, totalPages, page, pageSize)
            });
        }

        public async Task<IActionResult> GetAppointmentsAsync(SearchAppointmentDto form)
        {
            var query = _context.Appointments
                .Where(a => !a.IsDeleted)
                .Where(a => form.DoctorId == null || a.DoctorId == form.DoctorId)
                .Where(a => form.ClinicId == null || a.ClinicId == form.ClinicId)
                .Where(a => form.UserId == null || a.UserId == form.UserId)
                .Where(a => form.FromDate == null || a.AppointmentDate.Date >= form.FromDate.Value.ToDateTime(TimeOnly.MinValue))
                .Where(a => form.ToDate == null || a.AppointmentDate.Date <= form.ToDate.Value.ToDateTime(TimeOnly.MinValue))
                .Where(a => form.Status == null || a.Status == form.Status);

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.QueueNumber)
                .Select(a => new
                {
                    a.Id,
                    a.UserId,
                    Name = a.UserId.HasValue ? a.User.Name : a.GuestName,
                    PhoneNumber = a.UserId.HasValue ? a.User.PhoneNumber : a.GuestPhoneNumber,
                    a.DoctorId,
                    DoctorName = a.Doctor.Name,
                    a.ClinicId,
                    ClinicName = a.Clinic.Name,
                    a.AppointmentDate,
                    a.QueueNumber,
                    a.Code,
                    a.IsPhoneConfirmed,
                    Status = a.Status.ToString(),
                    a.PaymentAmount,
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

        public async Task<IActionResult> GetQueueAvailabilityAsync(int clinicId, DateOnly? fromDate, int days = 7)
        {
            if (days <= 0 || days > 31)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "Days must be between 1 and 31.",
                    Data = null
                });
            }

            var clinic = await _context.Clinics
                .Include(c => c.Doctor)
                .FirstOrDefaultAsync(c => c.Id == clinicId && !c.IsDeleted && c.IsVisible && !c.Doctor.IsDeleted);

            if (clinic == null)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "Clinic not found.",
                    Data = null
                });
            }

            var eBookingEnabled = await IsElectronicBookingEnabledAsync(clinic.DoctorId);

            if (!eBookingEnabled)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "Electronic booking is not enabled for this doctor.",
                    Data = null
                });
            }

            var startDate = fromDate ?? DateOnly.FromDateTime(DateTime.Today);
            if (startDate < DateOnly.FromDateTime(DateTime.Today))
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "Availability cannot be requested for a past date.",
                    Data = null
                });
            }

            var endDate = startDate.AddDays(days - 1);
            var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);
            var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue);

            var weeklyAvailability = await _context.DoctorAvailabilities
                .Include(a => a.Day)
                .Where(a => a.ClinicId == clinicId && !a.IsDeleted)
                .ToListAsync();

            var bookedAppointments = await _context.Appointments
                .Where(a =>
                    a.ClinicId == clinicId &&
                    a.AppointmentDate >= startDateTime &&
                    a.AppointmentDate <= endDateTime &&
                    a.Status != AppointmentStatus.Cancelled &&
                    !a.IsDeleted)
                .GroupBy(a => a.AppointmentDate.Date)
                .Select(group => new { Date = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => DateOnly.FromDateTime(item.Date), item => item.Count);

            var availability = Enumerable.Range(0, days)
                .Select(offset => startDate.AddDays(offset))
                .Select(date =>
                {
                    var day = weeklyAvailability.FirstOrDefault(a =>
                        a.Day.NormalizedName == date.DayOfWeek.ToString());
                    var booked = bookedAppointments.GetValueOrDefault(date);
                    var maxAppointments = day?.MaxAppointments ?? 0;
                    var isAvailable = day is { IsAvailable: true } && booked < maxAppointments;

                    return new QueueAvailabilityDto
                    {
                        ClinicId = clinicId,
                        Date = date,
                        DayName = day?.Day.Name ?? date.DayOfWeek.ToString(),
                        DayNormalizedName = date.DayOfWeek.ToString(),
                        StartTime = day?.StartTime,
                        EndTime = day?.EndTime,
                        MaxAppointments = maxAppointments,
                        BookedAppointments = booked,
                        RemainingAppointments = Math.Max(0, maxAppointments - booked),
                        IsAvailable = isAvailable
                    };
                })
                .ToList();

            return new OkObjectResult(new ResponseDto<List<QueueAvailabilityDto>>
            {
                Status = "Success",
                Code = 200,
                Message = "Queue availability retrieved successfully.",
                Data = availability
            });
        }

        public async Task<IActionResult> GetGuestAppointmentAsync(string phoneNumber, string code)
        {
            var normalizedPhoneNumber = phoneNumber?.Trim();
            var normalizedCode = code?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedPhoneNumber) || string.IsNullOrWhiteSpace(normalizedCode))
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "Phone number and booking code are required.",
                    Data = null
                });
            }

            var appointment = await _context.Appointments
                .Where(a =>
                    !a.IsDeleted &&
                    a.UserId == null &&
                    a.GuestPhoneNumber == normalizedPhoneNumber &&
                    a.Code == normalizedCode)
                .Select(a => new
                {
                    a.Id,
                    a.Code,
                    a.GuestName,
                    a.GuestPhoneNumber,
                    a.AppointmentDate,
                    a.QueueNumber,
                    a.Status,
                    a.IsPhoneConfirmed,
                    a.CancellationReason,
                    a.CancelledAt,
                    a.DoctorId,
                    DoctorName = a.Doctor.Name,
                    a.ClinicId,
                    ClinicName = a.Clinic.Name,
                    ClinicAddress = a.Clinic.Address,
                    ClinicPhoneNumber = a.Clinic.PhoneNumber,
                    a.Clinic.MapUrl,
                    a.Clinic.Latitude,
                    a.Clinic.Longitude
                })
                .FirstOrDefaultAsync();

            if (appointment == null)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "Guest booking not found.",
                    Data = null
                });
            }

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "Guest booking retrieved successfully.",
                Data = appointment
            });
        }

        public async Task<IActionResult> CreateAppointmentAsync(AddAppointmentDto form)
        {
            var clinic = await _context.Clinics
                .Include(c => c.Doctor)
                .FirstOrDefaultAsync(c =>
                    c.Id == form.ClinicId &&
                    c.DoctorId == form.DoctorId &&
                    !c.IsDeleted &&
                    c.IsVisible &&
                    !c.Doctor.IsDeleted);

            if (clinic == null)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "العيادة غير موجودة أو لا تتبع الطبيب المحدد.",
                    Data = null
                });
            }

            var eBookingEnabled = await IsElectronicBookingEnabledAsync(clinic.DoctorId);

            if (!eBookingEnabled)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "الحجز الإلكتروني غير مفعّل لهذا الطبيب.",
                    Data = null
                });
            }

            if (form.AppointmentDate.Date < DateTime.Today)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "لا يمكن الحجز بتاريخ سابق.",
                    Data = null
                });
            }

            var currentUserId = _load.GetCurrentUserId();
            var userId = currentUserId == Guid.Empty ? null : currentUserId;
            if (!userId.HasValue &&
                (string.IsNullOrWhiteSpace(form.GuestName) || string.IsNullOrWhiteSpace(form.GuestPhoneNumber)))
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "يرجى إدخال اسم ورقم هاتف الزائر.",
                    Data = null
                });
            }

            var dayName = form.AppointmentDate.DayOfWeek.ToString();
            var availability = await _context.DoctorAvailabilities
                .Include(a => a.Day)
                .FirstOrDefaultAsync(a =>
                    a.ClinicId == form.ClinicId &&
                    a.Day.NormalizedName == dayName &&
                    a.IsAvailable &&
                    !a.IsDeleted);

            if (availability == null)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "العيادة غير متوفرة في هذا اليوم.",
                    Data = null
                });
            }

            var appointmentDate = form.AppointmentDate.Date;
            var activeAppointments = _context.Appointments.Where(a =>
                a.ClinicId == form.ClinicId &&
                a.AppointmentDate.Date == appointmentDate &&
                a.Status != AppointmentStatus.Cancelled &&
                !a.IsDeleted);

            var existingAppointmentsCount = await activeAppointments.CountAsync();
            if (existingAppointmentsCount >= availability.MaxAppointments)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "تم الوصول إلى الحد الأقصى للأدوار في هذه العيادة لهذا اليوم.",
                    Data = null
                });
            }

            var guestPhoneNumber = form.GuestPhoneNumber?.Trim();
            var hasDuplicate = userId.HasValue
                ? await activeAppointments.AnyAsync(a => a.UserId == userId)
                : await activeAppointments.AnyAsync(a => a.GuestPhoneNumber == guestPhoneNumber);

            if (hasDuplicate)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "يوجد حجز مسبق في هذه العيادة لنفس اليوم.",
                    Data = null
                });
            }

            string uniqueCode;
            do
            {
                uniqueCode = GenerateRandomCode();
            } while (await _context.Appointments.AnyAsync(a => a.Code == uniqueCode));

            var queueNumber = await _context.Appointments
                .Where(a =>
                    a.ClinicId == form.ClinicId &&
                    a.AppointmentDate.Date == appointmentDate)
                .Select(a => (int?)a.QueueNumber)
                .MaxAsync() ?? 0;

            var appointment = new Appointment
            {
                UserId = userId,
                DoctorId = clinic.DoctorId,
                ClinicId = clinic.Id,
                AppointmentDate = appointmentDate,
                QueueNumber = queueNumber + 1,
                GuestName = userId.HasValue ? null : form.GuestName?.Trim(),
                GuestPhoneNumber = userId.HasValue ? null : guestPhoneNumber,
                Notes = form.Notes?.Trim(),
                IsPhoneConfirmed = true, // OTP becomes configurable in its dedicated phase.
                Status = AppointmentStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                CreatorId = userId,
                Code = uniqueCode
            };

            _context.Appointments.Add(appointment);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return new ConflictObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 409,
                    Message = "تعذر تثبيت الدور بسبب حجز متزامن. يرجى إعادة المحاولة.",
                    Data = null
                });
            }

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "تم إنشاء الحجز بنجاح.",
                Data = new { AppointmentId = appointment.Id, appointment.Code, appointment.QueueNumber }
            });
        }

        public async Task<IActionResult> CancelGuestAppointmentAsync(CancelGuestAppointmentDto form)
        {
            if (string.IsNullOrWhiteSpace(form.PhoneNumber) || string.IsNullOrWhiteSpace(form.Code))
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "Phone number and booking code are required.",
                    Data = null
                });
            }

            var appointment = await _context.Appointments.FirstOrDefaultAsync(a =>
                !a.IsDeleted &&
                a.UserId == null &&
                a.GuestPhoneNumber == form.PhoneNumber.Trim() &&
                a.Code == form.Code.Trim());

            if (appointment == null)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "Guest booking not found.",
                    Data = null
                });
            }

            return await CancelAppointmentAsync(appointment, form.Reason, null);
        }

        public async Task<IActionResult> CancelMyAppointmentAsync(CancelMyAppointmentDto form)
        {
            var userId = _load.GetCurrentUserId();
            if (!userId.HasValue || userId == Guid.Empty)
            {
                return new UnauthorizedObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 401,
                    Message = "Login is required.",
                    Data = null
                });
            }

            var appointment = await _context.Appointments.FirstOrDefaultAsync(a =>
                !a.IsDeleted &&
                a.Id == form.AppointmentId &&
                a.UserId == userId);

            if (appointment == null)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "Booking not found.",
                    Data = null
                });
            }

            return await CancelAppointmentAsync(appointment, form.Reason, userId);
        }

        public async Task<IActionResult> ToggleAppointmentStatusAsync(int appointmentId)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
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

            if (appointment.Status == AppointmentStatus.Completed)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "لا يمكن تغيير حالة الحجز المكتمل.",
                    Data = null
                });
            }

            appointment.Status = appointment.Status == AppointmentStatus.Confirmed
                ? AppointmentStatus.Cancelled
                : AppointmentStatus.Confirmed;
            appointment.ModifiedAt = DateTime.UtcNow;
            appointment.ModifierId = _load.GetCurrentUserId();

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                appointment.CancelledAt = DateTime.UtcNow;
                appointment.CancelledByUserId = _load.GetCurrentUserId();
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
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == appointmentId);
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
                    Message = "لا يمكن إكمال الحجز إلا إذا كان مؤكداً.",
                    Data = null
                });
            }

            appointment.Status = AppointmentStatus.Completed;
            appointment.ModifiedAt = DateTime.UtcNow;
            appointment.ModifierId = _load.GetCurrentUserId();
            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "تم تحديث حالة الحجز إلى مكتمل.",
                Data = null
            });
        }

        private static string GenerateRandomCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(chars => chars[Random.Shared.Next(chars.Length)])
                .ToArray());
        }

        private async Task<bool> IsElectronicBookingEnabledAsync(int doctorId)
        {
            var now = DateTime.UtcNow;
            var hasActivePackage = await _context.DoctorSubscriptions
                .AnyAsync(subscription =>
                    subscription.DoctorId == doctorId &&
                    subscription.StartDate <= now &&
                    subscription.EndDate >= now &&
                    subscription.Package.EBooking);

            if (!hasActivePackage)
            {
                return false;
            }

            return await _context.DoctorFeature
                .AnyAsync(feature =>
                    feature.DoctorId == doctorId &&
                    feature.Feature.NormalizedName == "EBooking" &&
                    feature.IsEnabled &&
                    !feature.IsDeleted);
        }

        private async Task<IActionResult> CancelAppointmentAsync(
            Appointment appointment, string? reason, Guid? cancelledByUserId)
        {
            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "Booking is already cancelled.",
                    Data = null
                });
            }

            if (appointment.Status == AppointmentStatus.Completed)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "Completed booking cannot be cancelled.",
                    Data = null
                });
            }

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancellationReason = reason?.Trim();
            appointment.CancelledAt = DateTime.UtcNow;
            appointment.CancelledByUserId = cancelledByUserId;
            appointment.ModifiedAt = DateTime.UtcNow;
            appointment.ModifierId = cancelledByUserId;
            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "Booking cancelled successfully.",
                Data = null
            });
        }
    }
}
