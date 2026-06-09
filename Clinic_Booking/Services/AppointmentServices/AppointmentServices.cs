using Clinic_Booking.Data;
using Clinic_Booking.Configuration;
using Clinic_Booking.DTOs.AppointmentDTO;
using Clinic_Booking.DTOs.ClinicDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.Appointment;
using Clinic_Booking.Entities.BookingOtpRequest;
using Clinic_Booking.Enums;
using Clinic_Booking.Extensions;
using Clinic_Booking.IServices.IAppointmentServices;
using Clinic_Booking.IServices.IBookingSmsServices;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.IPushNotificationServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Clinic_Booking.Services.AppointmentServices
{
    public class AppointmentServices : IAppointmentServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;
        private readonly IBookingSmsServices _bookingSmsServices;
        private readonly IPushNotificationServices _pushNotificationServices;
        private readonly BookingOtpOptions _bookingOtpOptions;
        private readonly ILogger<AppointmentServices> _logger;

        public AppointmentServices(
            ApplicationDbContext context,
            ILoadServices load,
            IBookingSmsServices bookingSmsServices,
            IPushNotificationServices pushNotificationServices,
            IOptions<BookingOtpOptions> bookingOtpOptions,
            ILogger<AppointmentServices> logger)
        {
            _context = context;
            _load = load;
            _bookingSmsServices = bookingSmsServices;
            _pushNotificationServices = pushNotificationServices;
            _bookingOtpOptions = bookingOtpOptions.Value;
            _logger = logger;
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
                        ConsultationPrice = a.Clinic.ConsultationPrice,
                        ShowConsultationPrice = a.Clinic.ShowConsultationPrice,
                        IsVisible = a.Clinic.IsVisible
                    },
                    AppointmentDate = a.AppointmentDate,
                    QueueNumber = a.QueueNumber,
                    GuestName = a.GuestName,
                    GuestPhoneNumber = a.GuestPhoneNumber,
                    IsGuestBooking = !a.UserId.HasValue,
                    BookingSource = a.UserId.HasValue ? "Registered" : "Guest",
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

        public async Task<IActionResult> GetMineForDoctorAsync(SearchAppointmentDto form)
        {
            var userId = GetAuthenticatedUserId();
            if (!userId.HasValue)
            {
                return LoginRequired();
            }

            var doctorId = await _context.Doctors
                .Where(doctor => doctor.UserId == userId && !doctor.IsDeleted)
                .Select(doctor => (int?)doctor.Id)
                .FirstOrDefaultAsync();
            if (!doctorId.HasValue)
            {
                return DoctorAccessDenied();
            }

            var appointments = await ProjectBookingDetails(_context.Appointments
                    .Where(appointment =>
                        !appointment.IsDeleted &&
                        appointment.DoctorId == doctorId &&
                        (form.ClinicId == null || appointment.ClinicId == form.ClinicId) &&
                        (form.FromDate == null || appointment.AppointmentDate.Date >= form.FromDate.Value.ToDateTime(TimeOnly.MinValue)) &&
                        (form.ToDate == null || appointment.AppointmentDate.Date <= form.ToDate.Value.ToDateTime(TimeOnly.MinValue)) &&
                        (form.Status == null || appointment.Status == form.Status)))
                .OrderBy(appointment => appointment.AppointmentDate)
                .ThenBy(appointment => appointment.QueueNumber)
                .ToListAsync();

            return new OkObjectResult(new ResponseDto<List<BookingDetailsDto>>
            {
                Status = "Success",
                Code = 200,
                Message = "Bookings retrieved successfully.",
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
            await ExpireUnconfirmedBookingsAsync(clinicId);

            var weeklyAvailability = await _context.DoctorAvailabilities
                .Include(a => a.Day)
                .Where(a => a.ClinicId == clinicId && !a.IsDeleted)
                .ToListAsync();

            var clinicExceptions = await _context.ClinicExceptions
                .Where(exception =>
                    exception.ClinicId == clinicId &&
                    exception.ExceptionDate >= startDateTime &&
                    exception.ExceptionDate <= endDateTime &&
                    !exception.IsDeleted)
                .ToDictionaryAsync(
                    exception => DateOnly.FromDateTime(exception.ExceptionDate),
                    exception => exception);

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
                    var clinicException = clinicExceptions.GetValueOrDefault(date);
                    var booked = bookedAppointments.GetValueOrDefault(date);
                    var maxAppointments = clinicException?.MaxAppointments ?? day?.MaxAppointments ?? 0;
                    var isAvailable = day is { IsAvailable: true } &&
                        clinicException?.IsClosed != true &&
                        booked < maxAppointments;

                    return new QueueAvailabilityDto
                    {
                        ClinicId = clinicId,
                        Date = date,
                        DayName = GetArabicDayName(date.DayOfWeek),
                        DayNormalizedName = date.DayOfWeek.ToString(),
                        StartTime = clinicException?.StartTime ?? day?.StartTime,
                        EndTime = clinicException?.EndTime ?? day?.EndTime,
                        MaxAppointments = maxAppointments,
                        BookedAppointments = booked,
                        RemainingAppointments = Math.Max(0, maxAppointments - booked),
                        IsAvailable = isAvailable,
                        HasException = clinicException != null,
                        ClosureReason = clinicException?.IsClosed == true
                            ? clinicException.ClosureReason
                            : null
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

        private static string GetArabicDayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => "الأحد",
                DayOfWeek.Monday => "الاثنين",
                DayOfWeek.Tuesday => "الثلاثاء",
                DayOfWeek.Wednesday => "الأربعاء",
                DayOfWeek.Thursday => "الخميس",
                DayOfWeek.Friday => "الجمعة",
                DayOfWeek.Saturday => "السبت",
                _ => dayOfWeek.ToString()
            };
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

            var appointment = await ProjectBookingDetails(
                _context.Appointments
                .Where(a =>
                    !a.IsDeleted &&
                    a.UserId == null &&
                    a.GuestPhoneNumber == normalizedPhoneNumber &&
                    a.Code == normalizedCode))
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

        public async Task<IActionResult> GetMyAppointmentsAsync()
        {
            var userId = GetAuthenticatedUserId();
            if (!userId.HasValue)
            {
                return LoginRequired();
            }

            var appointments = await ProjectBookingDetails(
                    _context.Appointments.Where(a => !a.IsDeleted && a.UserId == userId))
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.QueueNumber)
                .ToListAsync();

            return new OkObjectResult(new ResponseDto<List<BookingDetailsDto>>
            {
                Status = "Success",
                Code = 200,
                Message = "Bookings retrieved successfully.",
                Data = appointments
            });
        }

        public async Task<IActionResult> GetMyAppointmentAsync(int appointmentId)
        {
            if (appointmentId <= 0)
            {
                return InvalidBookingId();
            }

            var userId = GetAuthenticatedUserId();
            if (!userId.HasValue)
            {
                return LoginRequired();
            }

            var appointment = await ProjectBookingDetails(
                    _context.Appointments.Where(a =>
                        !a.IsDeleted &&
                        a.Id == appointmentId &&
                        a.UserId == userId))
                .FirstOrDefaultAsync();

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

            return new OkObjectResult(new ResponseDto<BookingDetailsDto>
            {
                Status = "Success",
                Code = 200,
                Message = "Booking retrieved successfully.",
                Data = appointment
            });
        }

        public async Task<IActionResult> CreateAppointmentAsync(AddAppointmentDto form)
        {
            if (form.DoctorId <= 0 || form.ClinicId <= 0)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "Doctor and clinic are required.",
                    Data = null
                });
            }

            if (form.Notes?.Length > 1000)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "Notes cannot exceed 1000 characters.",
                    Data = null
                });
            }

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

            var userId = GetAuthenticatedUserId();
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

            if (!userId.HasValue && (form.GuestName!.Length > 200 || form.GuestPhoneNumber!.Length > 30))
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "Guest name or phone number is too long.",
                    Data = null
                });
            }

            var bookingUser = userId.HasValue
                ? await _context.AspNetUsers
                    .Where(user => user.Id == userId)
                    .Select(user => new
                    {
                        user.PhoneNumber,
                        user.PhoneNumberConfirmed,
                        user.EmailConfirmed
                    })
                    .FirstOrDefaultAsync()
                : null;

            if (userId.HasValue &&
            bookingUser is { PhoneNumberConfirmed: false, EmailConfirmed: false })
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "يجب تأكيد رقم الهاتف أو البريد الإلكتروني قبل الحجز.",
                    Data = null
                });
            }

            var bookingPhoneNumber = userId.HasValue
                ? bookingUser?.PhoneNumber
                : form.GuestPhoneNumber?.Trim();

            var requiresOtp = !userId.HasValue || _bookingOtpOptions.Enabled;

            if (requiresOtp && string.IsNullOrWhiteSpace(bookingPhoneNumber))
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "Phone number is required to confirm the booking.",
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
            var clinicException = await _context.ClinicExceptions
                .FirstOrDefaultAsync(exception =>
                    exception.ClinicId == form.ClinicId &&
                    exception.ExceptionDate == appointmentDate &&
                    !exception.IsDeleted);

            if (clinicException?.IsClosed == true)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = string.IsNullOrWhiteSpace(clinicException.ClosureReason)
                        ? "Clinic is closed on the selected date."
                        : $"Clinic is closed on the selected date: {clinicException.ClosureReason}",
                    Data = null
                });
            }

            await ExpireUnconfirmedBookingsAsync(form.ClinicId);
            var activeAppointments = _context.Appointments.Where(a =>
                a.ClinicId == form.ClinicId &&
                a.AppointmentDate.Date == appointmentDate &&
                a.Status != AppointmentStatus.Cancelled &&
                !a.IsDeleted);

            var existingAppointmentsCount = await activeAppointments.CountAsync();
            var maxAppointments = clinicException?.MaxAppointments ?? availability.MaxAppointments;
            if (existingAppointmentsCount >= maxAppointments)
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
                ? await _context.Appointments.AnyAsync(a =>
                    a.UserId == userId &&
                    a.Status != AppointmentStatus.Cancelled &&
                    !a.IsDeleted)
                : await _context.Appointments.AnyAsync(a =>
                    a.GuestPhoneNumber == guestPhoneNumber &&
                    a.Status != AppointmentStatus.Cancelled &&
                    !a.IsDeleted);

            if (hasDuplicate)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = userId.HasValue
                        ? "لديك حجز فعال مسبقاً، لا يمكنك إنشاء أكثر من حجز."
                        : "يوجد حجز فعال مسبقاً لهذا الرقم، لا يمكن إنشاء أكثر من حجز.",
                    Data = null
                });
            }

            Appointment appointment;
            try
            {
                appointment = await CreateQueuedAppointmentAsync(
                    clinic.Id,
                    appointmentDate,
                    queueNumber => new Appointment
                    {
                        UserId = userId,
                        DoctorId = clinic.DoctorId,
                        ClinicId = clinic.Id,
                        AppointmentDate = appointmentDate,
                        QueueNumber = queueNumber,
                        GuestName = userId.HasValue ? null : form.GuestName?.Trim(),
                        GuestPhoneNumber = userId.HasValue ? null : guestPhoneNumber,
                        Notes = form.Notes?.Trim(),
                        IsPhoneConfirmed = !requiresOtp,
                        Status = AppointmentStatus.Pending,
                        PaymentStatus = PaymentStatus.Pending,
                        CreatorId = userId,
                        Code = GenerateRandomCode()
                    });

                if (requiresOtp)
                {
                    try
                    {
                        await CreateAndSendBookingOtpAsync(appointment, bookingPhoneNumber!);
                    }
                    catch
                    {
                        appointment.Status = AppointmentStatus.Cancelled;
                        appointment.CancellationReason = "OTP delivery failed.";
                        appointment.CancelledAt = DateTime.UtcNow;
                        appointment.ModifiedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        return OtpDeliveryFailed();
                    }
                }
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

            await NotifyDoctorAsync(
                appointment.DoctorId,
                "حجز جديد",
                requiresOtp
                    ? "تم إنشاء حجز جديد وينتظر تأكيد رقم الهاتف."
                    : "تم إنشاء حجز جديد.",
                appointment);

            if (!requiresOtp)
            {
                var doctorName = await GetDoctorNameAsync(appointment.DoctorId);
                var appointmentDateText = FormatAppointmentDate(appointment.AppointmentDate);
                await NotifyPatientAsync(
                    appointment,
                    "تم استلام طلب الحجز",
                    $"تم استلام طلب حجزك لدى الدكتور {doctorName} بتاريخ {appointmentDateText} وهو بانتظار موافقة الطبيب.");
            }

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "تم إنشاء الحجز بنجاح.",
                Data = new
                {
                    AppointmentId = appointment.Id,
                    appointment.Code,
                    appointment.QueueNumber,
                    appointment.IsPhoneConfirmed,
                    RequiresOtp = requiresOtp
                }
            });
        }

        public async Task<IActionResult> CreateManualAppointmentAsync(ManualAppointmentDto form)
        {
            var userId = GetAuthenticatedUserId();
            if (!userId.HasValue)
            {
                return LoginRequired();
            }

            var clinic = await _context.Clinics
                .Include(c => c.Doctor)
                .FirstOrDefaultAsync(c =>
                    c.Id == form.ClinicId &&
                    !c.IsDeleted &&
                    !c.Doctor.IsDeleted &&
                    c.Doctor.UserId == userId);
            if (clinic == null)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "العيادة غير موجودة أو لا تتبع حساب الطبيب الحالي.",
                    Data = null
                });
            }

            var patientName = form.PatientName?.Trim();
            var patientPhoneNumber = form.PatientPhoneNumber?.Trim();
            if (string.IsNullOrWhiteSpace(patientName) || string.IsNullOrWhiteSpace(patientPhoneNumber))
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "اسم المراجع ورقم الهاتف مطلوبان.",
                    Data = null
                });
            }

            var appointmentDate = form.AppointmentDate.Date;
            if (appointmentDate < DateTime.Today)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "لا يمكن إضافة حجز يدوي بتاريخ سابق.",
                    Data = null
                });
            }

            var availability = await _context.DoctorAvailabilities
                .Include(a => a.Day)
                .FirstOrDefaultAsync(a =>
                    a.ClinicId == form.ClinicId &&
                    a.Day.NormalizedName == appointmentDate.DayOfWeek.ToString() &&
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

            var clinicException = await _context.ClinicExceptions
                .FirstOrDefaultAsync(exception =>
                    exception.ClinicId == form.ClinicId &&
                    exception.ExceptionDate == appointmentDate &&
                    !exception.IsDeleted);
            if (clinicException?.IsClosed == true)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = string.IsNullOrWhiteSpace(clinicException.ClosureReason)
                        ? "العيادة مغلقة في التاريخ المحدد."
                        : $"العيادة مغلقة في التاريخ المحدد: {clinicException.ClosureReason}",
                    Data = null
                });
            }

            await ExpireUnconfirmedBookingsAsync(form.ClinicId);
            var activeAppointments = _context.Appointments.Where(a =>
                a.ClinicId == form.ClinicId &&
                a.AppointmentDate.Date == appointmentDate &&
                a.Status != AppointmentStatus.Cancelled &&
                !a.IsDeleted);
            var maxAppointments = clinicException?.MaxAppointments ?? availability.MaxAppointments;
            if (await activeAppointments.CountAsync() >= maxAppointments)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "تم الوصول إلى الحد الأقصى للأدوار في هذه العيادة لهذا اليوم.",
                    Data = null
                });
            }

            if (await activeAppointments.AnyAsync(a =>
                a.GuestPhoneNumber == patientPhoneNumber ||
                (a.UserId.HasValue && a.User != null && a.User.PhoneNumber == patientPhoneNumber)))
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "يوجد حجز مسبق في هذه العيادة لنفس الهاتف في اليوم المحدد.",
                    Data = null
                });
            }

            Appointment appointment;
            try
            {
                appointment = await CreateQueuedAppointmentAsync(
                    clinic.Id,
                    appointmentDate,
                    queueNumber => new Appointment
                    {
                        DoctorId = clinic.DoctorId,
                        ClinicId = clinic.Id,
                        AppointmentDate = appointmentDate,
                        QueueNumber = queueNumber,
                        GuestName = patientName,
                        GuestPhoneNumber = patientPhoneNumber,
                        Notes = form.Notes?.Trim(),
                        IsPhoneConfirmed = true,
                        Status = AppointmentStatus.Confirmed,
                        PaymentStatus = PaymentStatus.Pending,
                        CreatorId = userId,
                        Code = GenerateRandomCode()
                    });
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
                Message = "تمت إضافة الحجز اليدوي بنجاح.",
                Data = new { AppointmentId = appointment.Id, appointment.Code, appointment.QueueNumber }
            });
        }

        public async Task<IActionResult> ResendBookingOtpAsync(ResendBookingOtpDto form)
        {
            var appointment = await GetOtpAppointmentAsync(form.PhoneNumber, form.BookingCode);
            if (appointment == null)
            {
                return BookingNotFound();
            }

            if (!IsOtpRequiredForAppointment(appointment))
            {
                return OtpDisabled();
            }

            if (appointment.IsPhoneConfirmed)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "Phone number is already confirmed.",
                    Data = null
                });
            }

            var lastRequest = await _context.BookingOtpRequests
                .Where(request => request.AppointmentId == appointment.Id)
                .OrderByDescending(request => request.SentAt)
                .FirstOrDefaultAsync();

            if (lastRequest != null && lastRequest.ExpiresAt < DateTime.UtcNow)
            {
                await ExpireUnconfirmedBookingsAsync(appointment.ClinicId);
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "Booking OTP expired. Please create a new booking.",
                    Data = null
                });
            }

            if (lastRequest != null)
            {
                var resendAvailableAt = lastRequest.SentAt.AddSeconds(_bookingOtpOptions.ResendCooldownSeconds);
                if (resendAvailableAt > DateTime.UtcNow)
                {
                    return new BadRequestObjectResult(new ResponseDto<object>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "Please wait before requesting another OTP.",
                        Data = new { ResendAvailableAt = resendAvailableAt }
                    });
                }
            }

            try
            {
                await CreateAndSendBookingOtpAsync(appointment, form.PhoneNumber.Trim());
            }
            catch
            {
                return OtpDeliveryFailed();
            }
            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "OTP sent successfully.",
                Data = null
            });
        }

        public async Task<IActionResult> ConfirmBookingOtpAsync(BookingOtpDto form)
        {
            if (string.IsNullOrWhiteSpace(form.OtpCode))
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "OTP code is required.",
                    Data = null
                });
            }

            var appointment = await GetOtpAppointmentAsync(form.PhoneNumber, form.BookingCode);
            if (appointment == null)
            {
                return BookingNotFound();
            }

            if (!IsOtpRequiredForAppointment(appointment))
            {
                return OtpDisabled();
            }

            if (appointment.IsPhoneConfirmed)
            {
                return new OkObjectResult(new ResponseDto<object>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "Phone number is already confirmed.",
                    Data = null
                });
            }

            var otpRequest = await _context.BookingOtpRequests
                .Where(request => request.AppointmentId == appointment.Id && !request.IsUsed)
                .OrderByDescending(request => request.SentAt)
                .FirstOrDefaultAsync();

            if (otpRequest == null || otpRequest.ExpiresAt < DateTime.UtcNow)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "OTP code is expired. Please request a new one.",
                    Data = null
                });
            }

            if (otpRequest.AttemptCount >= _bookingOtpOptions.MaxAttempts)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "Maximum OTP attempts reached. Please request a new one.",
                    Data = null
                });
            }

            otpRequest.AttemptCount++;
            if (!CryptographicOperations.FixedTimeEquals(
                    Convert.FromHexString(otpRequest.CodeHash),
                    Convert.FromHexString(HashOtpCode(form.OtpCode.Trim(), otpRequest.CodeSalt))))
            {
                await _context.SaveChangesAsync();
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "Invalid OTP code.",
                    Data = new { RemainingAttempts = Math.Max(0, _bookingOtpOptions.MaxAttempts - otpRequest.AttemptCount) }
                });
            }

            otpRequest.IsUsed = true;
            otpRequest.VerifiedAt = DateTime.UtcNow;
            appointment.IsPhoneConfirmed = true;
            appointment.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await NotifyDoctorAsync(
                appointment.DoctorId,
                "تم تثبيت حجز جديد",
                $"أكد المراجع رقم الهاتف للحجز رقم {appointment.QueueNumber}.",
                appointment);

            var doctorName = await GetDoctorNameAsync(appointment.DoctorId);
            var appointmentDate = FormatAppointmentDate(appointment.AppointmentDate);
            await NotifyPatientAsync(
                appointment,
                "تم تثبيت طلب الحجز",
                $"تم تثبيت طلب حجزك لدى الدكتور {doctorName} بتاريخ {appointmentDate} وهو بانتظار موافقة الطبيب.");

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "Booking phone number confirmed successfully.",
                Data = new { appointment.Id, appointment.Code, appointment.QueueNumber }
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

            return new BadRequestObjectResult(new ResponseDto<object>
            {
                Status = "Error",
                Code = 400,
                Message = "Guest booking cancellation is disabled. Please contact the clinic to cancel this booking.",
                Data = null
            });
        }

        public async Task<IActionResult> CancelMyAppointmentAsync(CancelMyAppointmentDto form)
        {
            if (form.AppointmentId <= 0)
            {
                return InvalidBookingId();
            }

            var userId = GetAuthenticatedUserId();
            if (!userId.HasValue)
            {
                return LoginRequired();
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

            if (!await IsOwnedByCurrentDoctorAsync(appointment.DoctorId))
            {
                return DoctorAccessDenied();
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

            if (appointment.Status != AppointmentStatus.Pending &&
                appointment.Status != AppointmentStatus.Confirmed)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "لا يمكن إعادة تفعيل حجز ملغي أو مكتمل.",
                    Data = null
                });
            }

            var oldStatus = appointment.Status;
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

            _logger.LogInformation(
                "Doctor toggled appointment status. AppointmentId={AppointmentId}, OldStatus={OldStatus}, NewStatus={NewStatus}, PatientUserId={PatientUserId}, DoctorId={DoctorId}",
                appointment.Id,
                oldStatus,
                appointment.Status,
                appointment.UserId,
                appointment.DoctorId);

            if (appointment.Status == AppointmentStatus.Confirmed)
            {
                var doctorName = await GetDoctorNameAsync(appointment.DoctorId);
                var appointmentDate = FormatAppointmentDate(appointment.AppointmentDate);
                await NotifyPatientAsync(
                    appointment,
                    "تمت الموافقة على الحجز",
                    $"تمت الموافقة على حجزك لدى الدكتور {doctorName} بتاريخ {appointmentDate}.");
            }
            else if (appointment.Status == AppointmentStatus.Cancelled)
            {
                await NotifyBookingCancelledAsync(appointment, includeDoctor: false);
            }

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

            if (!await IsOwnedByCurrentDoctorAsync(appointment.DoctorId))
            {
                return DoctorAccessDenied();
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

            var doctorName = await GetDoctorNameAsync(appointment.DoctorId);
            await NotifyPatientAsync(
                appointment,
                "شكراً لاستخدامك تطبيق عيادتي",
                $"يرجى من حضرتك تقييم الدكتور {doctorName} الذي قمت بزيارته مؤخراً.");

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "تم تحديث حالة الحجز إلى مكتمل.",
                Data = null
            });
        }

        private async Task<Appointment> CreateQueuedAppointmentAsync(
            int clinicId,
            DateTime appointmentDate,
            Func<int, Appointment> appointmentFactory)
        {
            const int maxAttempts = 3;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var queueNumber = await _context.Appointments
                    .Where(a => a.ClinicId == clinicId && a.AppointmentDate.Date == appointmentDate.Date)
                    .Select(a => (int?)a.QueueNumber)
                    .MaxAsync() ?? 0;

                var appointment = appointmentFactory(queueNumber + 1);
                _context.Appointments.Add(appointment);
                try
                {
                    await _context.SaveChangesAsync();
                    return appointment;
                }
                catch (DbUpdateException) when (attempt < maxAttempts)
                {
                    _context.Entry(appointment).State = EntityState.Detached;
                }
            }

            throw new DbUpdateException("Unable to allocate a queue number after retrying concurrent booking collisions.");
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
                    subscription.Status == SubscriptionStatus.Active &&
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

        private async Task<Appointment?> GetOtpAppointmentAsync(string phoneNumber, string bookingCode)
        {
            var normalizedPhoneNumber = phoneNumber?.Trim();
            var normalizedBookingCode = bookingCode?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedPhoneNumber) || string.IsNullOrWhiteSpace(normalizedBookingCode))
            {
                return null;
            }

            return await _context.Appointments
                .Include(appointment => appointment.User)
                .FirstOrDefaultAsync(appointment =>
                    !appointment.IsDeleted &&
                    appointment.Status != AppointmentStatus.Cancelled &&
                    appointment.Code == normalizedBookingCode &&
                    (appointment.UserId.HasValue
                        ? appointment.User.PhoneNumber == normalizedPhoneNumber
                        : appointment.GuestPhoneNumber == normalizedPhoneNumber));
        }

        private bool IsOtpRequiredForAppointment(Appointment appointment)
        {
            return !appointment.UserId.HasValue || _bookingOtpOptions.Enabled;
        }

        private async Task CreateAndSendBookingOtpAsync(Appointment appointment, string phoneNumber)
        {
            var oldRequests = await _context.BookingOtpRequests
                .Where(request => request.AppointmentId == appointment.Id && !request.IsUsed)
                .ToListAsync();

            foreach (var request in oldRequests)
            {
                request.IsUsed = true;
            }

            var otpCode = GenerateNumericOtp(_bookingOtpOptions.CodeLength);

            var codeSalt = GenerateOtpSalt();
            var now = DateTime.UtcNow;
            var otpRequest = new BookingOtpRequest
            {
                AppointmentId = appointment.Id,
                PhoneNumber = phoneNumber,
                CodeHash = HashOtpCode(otpCode, codeSalt),
                CodeSalt = codeSalt,
                SentAt = now,
                ExpiresAt = now.AddMinutes(_bookingOtpOptions.ExpirationMinutes)
            };
            _context.BookingOtpRequests.Add(otpRequest);

            await _context.SaveChangesAsync();
            try
            {
                await _bookingSmsServices.SendBookingOtpAsync(phoneNumber, otpCode);
            }
            catch
            {
                otpRequest.IsUsed = true;
                await _context.SaveChangesAsync();
                throw;
            }
        }

        private async Task ExpireUnconfirmedBookingsAsync(int clinicId)
        {
            var now = DateTime.UtcNow;
            var expiredAppointments = await _context.Appointments
                .Where(appointment =>
                    appointment.ClinicId == clinicId &&
                    !appointment.IsDeleted &&
                    !appointment.IsPhoneConfirmed &&
                    appointment.Status == AppointmentStatus.Pending &&
                    (!appointment.UserId.HasValue || _bookingOtpOptions.Enabled) &&
                    _context.BookingOtpRequests.Any(request =>
                        request.AppointmentId == appointment.Id &&
                        !request.IsUsed &&
                        request.ExpiresAt < now))
                .ToListAsync();

            foreach (var appointment in expiredAppointments)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                appointment.CancellationReason = "Booking OTP expired.";
                appointment.CancelledAt = now;
                appointment.ModifiedAt = now;
            }

            if (expiredAppointments.Count > 0)
            {
                await _context.SaveChangesAsync();
            }
        }

        private static string GenerateNumericOtp(int length)
        {
            var safeLength = Math.Clamp(length, 4, 8);
            var minValue = (int)Math.Pow(10, safeLength - 1);
            var maxValue = (int)Math.Pow(10, safeLength);
            return RandomNumberGenerator.GetInt32(minValue, maxValue).ToString();
        }

        private static string GenerateOtpSalt()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        }

        private static string HashOtpCode(string code, string salt)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{salt}:{code}")));
        }

        private Guid? GetAuthenticatedUserId()
        {
            var userId = _load.GetCurrentUserId();
            return userId.HasValue && userId != Guid.Empty ? userId : null;
        }

        private async Task<bool> IsOwnedByCurrentDoctorAsync(int doctorId)
        {
            var userId = GetAuthenticatedUserId();
            return userId.HasValue && await _context.Doctors.AnyAsync(doctor =>
                doctor.Id == doctorId &&
                doctor.UserId == userId &&
                !doctor.IsDeleted);
        }

        private async Task NotifyDoctorAsync(int doctorId, string title, string body, Appointment appointment)
        {
            var doctorUserId = await _context.Doctors
                .Where(doctor => doctor.Id == doctorId && !doctor.IsDeleted && doctor.UserId.HasValue)
                .Select(doctor => doctor.UserId)
                .FirstOrDefaultAsync();

            if (!doctorUserId.HasValue)
            {
                return;
            }

            await _pushNotificationServices.SendToUserAsync(
                doctorUserId.Value,
                title,
                body,
                BookingNotificationData(appointment));
        }

        private async Task NotifyPatientAsync(Appointment appointment, string title, string body)
        {
            if (!appointment.UserId.HasValue)
            {
                _logger.LogInformation(
                    "Patient push notification skipped. AppointmentId={AppointmentId}, Title={Title}. Guest bookings are not linked to an app user.",
                    appointment.Id,
                    title);
                return;
            }

            _logger.LogInformation(
                "Sending patient push notification. AppointmentId={AppointmentId}, PatientUserId={PatientUserId}, Title={Title}",
                appointment.Id,
                appointment.UserId,
                title);

            await _pushNotificationServices.SendToUserAsync(
                appointment.UserId.Value,
                title,
                body,
                BookingNotificationData(appointment));
        }

        private async Task NotifyBookingCancelledAsync(Appointment appointment, bool includeDoctor)
        {
            var doctorName = await GetDoctorNameAsync(appointment.DoctorId);
            var appointmentDate = FormatAppointmentDate(appointment.AppointmentDate);

            await NotifyPatientAsync(
                appointment,
                "تم إلغاء الحجز",
                $"تم إلغاء حجزك لدى الدكتور {doctorName} بتاريخ {appointmentDate}.");

            if (includeDoctor)
            {
                await NotifyDoctorAsync(
                    appointment.DoctorId,
                    "تم إلغاء الحجز",
                    $"تم إلغاء الحجز رقم {appointment.QueueNumber} بتاريخ {appointmentDate}.",
                    appointment);
            }
        }

        private async Task NotifyBookingPartiesAsync(
            Appointment appointment,
            string title,
            string body,
            bool includePatient)
        {
            var notifiedUsers = new HashSet<Guid>();
            var doctorUserId = await _context.Doctors
                .Where(doctor => doctor.Id == appointment.DoctorId && !doctor.IsDeleted && doctor.UserId.HasValue)
                .Select(doctor => doctor.UserId)
                .FirstOrDefaultAsync();

            _logger.LogInformation(
                "Booking notification routing. AppointmentId={AppointmentId}, DoctorUserId={DoctorUserId}, PatientUserId={PatientUserId}, IncludePatient={IncludePatient}",
                appointment.Id,
                doctorUserId,
                appointment.UserId,
                includePatient);

            if (doctorUserId.HasValue && notifiedUsers.Add(doctorUserId.Value))
            {
                await _pushNotificationServices.SendToUserAsync(
                    doctorUserId.Value,
                    title,
                    body,
                    BookingNotificationData(appointment));
            }
            else
            {
                _logger.LogInformation(
                    "Booking notification did not send to doctor. AppointmentId={AppointmentId}, DoctorUserId={DoctorUserId}",
                    appointment.Id,
                    doctorUserId);
            }

            if (includePatient && appointment.UserId.HasValue && notifiedUsers.Add(appointment.UserId.Value))
            {
                await _pushNotificationServices.SendToUserAsync(
                    appointment.UserId.Value,
                    title,
                    body,
                    BookingNotificationData(appointment));
            }
            else if (includePatient)
            {
                _logger.LogInformation(
                    "Booking notification did not send to patient. AppointmentId={AppointmentId}, PatientUserId={PatientUserId}. Guest bookings cannot receive push notifications unless linked to an app user.",
                    appointment.Id,
                    appointment.UserId);
            }
        }

        private async Task<string> GetDoctorNameAsync(int doctorId)
        {
            return await _context.Doctors
                .Where(doctor => doctor.Id == doctorId && !doctor.IsDeleted)
                .Select(doctor => doctor.Name)
                .FirstOrDefaultAsync()
                ?? "الطبيب";
        }

        private static string FormatAppointmentDate(DateTime appointmentDate)
        {
            return appointmentDate.ToString("yyyy/MM/dd");
        }

        private static Dictionary<string, string> BookingNotificationData(Appointment appointment)
        {
            return new Dictionary<string, string>
            {
                ["type"] = "booking",
                ["appointmentId"] = appointment.Id.ToString(),
                ["doctorId"] = appointment.DoctorId.ToString(),
                ["clinicId"] = appointment.ClinicId.ToString(),
                ["queueNumber"] = appointment.QueueNumber.ToString(),
                ["status"] = appointment.Status.ToString()
            };
        }

        private static UnauthorizedObjectResult DoctorAccessDenied()
        {
            return new UnauthorizedObjectResult(new ResponseDto<object>
            {
                Status = "Error",
                Code = 401,
                Message = "You do not have permission to manage this booking.",
                Data = null
            });
        }

        private static IQueryable<BookingDetailsDto> ProjectBookingDetails(IQueryable<Appointment> query)
        {
            return query.Select(appointment => new BookingDetailsDto
            {
                Id = appointment.Id,
                Code = appointment.Code,
                PatientName = appointment.UserId.HasValue
                    ? appointment.User!.Name ?? string.Empty
                    : appointment.GuestName ?? string.Empty,
                PatientPhoneNumber = appointment.UserId.HasValue
                    ? appointment.User!.PhoneNumber
                    : appointment.GuestPhoneNumber,
                AppointmentDate = appointment.AppointmentDate,
                QueueNumber = appointment.QueueNumber,
                Status = appointment.Status,
                IsPhoneConfirmed = appointment.IsPhoneConfirmed,
                IsGuestBooking = !appointment.UserId.HasValue,
                BookingSource = appointment.UserId.HasValue ? "Registered" : "Guest",
                HasReview = appointment.Reviews.Any(review => !review.IsDeleted),
                CancellationReason = appointment.CancellationReason,
                CancelledAt = appointment.CancelledAt,
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor.Name,
                ClinicId = appointment.ClinicId,
                ClinicName = appointment.Clinic.Name,
                ClinicAddress = appointment.Clinic.Address,
                ClinicPhoneNumber = appointment.Clinic.PhoneNumber,
                MapUrl = appointment.Clinic.MapUrl,
                Latitude = appointment.Clinic.Latitude,
                Longitude = appointment.Clinic.Longitude
            });
        }

        private static UnauthorizedObjectResult LoginRequired()
        {
            return new UnauthorizedObjectResult(new ResponseDto<object>
            {
                Status = "Error",
                Code = 401,
                Message = "Login is required.",
                Data = null
            });
        }

        private static BadRequestObjectResult InvalidBookingId()
        {
            return new BadRequestObjectResult(new ResponseDto<object>
            {
                Status = "Error",
                Code = 400,
                Message = "Invalid booking id.",
                Data = null
            });
        }

        private static BadRequestObjectResult OtpDisabled()
        {
            return new BadRequestObjectResult(new ResponseDto<object>
            {
                Status = "Error",
                Code = 400,
                Message = "Booking OTP is disabled.",
                Data = null
            });
        }

        private static NotFoundObjectResult BookingNotFound()
        {
            return new NotFoundObjectResult(new ResponseDto<object>
            {
                Status = "Error",
                Code = 404,
                Message = "Booking not found.",
                Data = null
            });
        }

        private static ObjectResult OtpDeliveryFailed()
        {
            return new ObjectResult(new ResponseDto<object>
            {
                Status = "Error",
                Code = 503,
                Message = "OTP delivery failed. Please try again later.",
                Data = null
            })
            {
                StatusCode = 503
            };
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

            await NotifyBookingCancelledAsync(appointment, includeDoctor: true);

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
