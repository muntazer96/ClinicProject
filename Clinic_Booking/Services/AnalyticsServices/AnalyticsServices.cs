using Clinic_Booking.Data;
using Clinic_Booking.DTOs.AnalyticsDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.Analytics;
using Clinic_Booking.Enums;
using Clinic_Booking.Extensions;
using Clinic_Booking.IServices.IAnalyticsServices;
using Clinic_Booking.IServices.ILoadServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.AnalyticsServices
{
    public class AnalyticsServices : IAnalyticsServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;

        public AnalyticsServices(ApplicationDbContext context, ILoadServices load)
        {
            _context = context;
            _load = load;
        }

        public async Task<IActionResult> TrackAsync(TrackAnalyticsEventDto form)
        {
            if (string.IsNullOrWhiteSpace(form.EventType))
            {
                return Error("نوع الحدث مطلوب.");
            }

            var userId = _load.GetCurrentUserId();
            if (userId == Guid.Empty) userId = null;

            _context.AnalyticsEvents.Add(new AnalyticsEvent
            {
                EventType = TrimOrNull(form.EventType, 80)!.ToLowerInvariant(),
                UserId = userId,
                IsGuest = !userId.HasValue,
                DoctorId = form.DoctorId,
                ClinicId = form.ClinicId,
                SpecializationId = form.SpecializationId,
                AppointmentId = form.AppointmentId,
                OfferId = form.OfferId,
                Source = TrimOrNull(form.Source, 80),
                Platform = TrimOrNull(form.Platform, 40),
                Page = TrimOrNull(form.Page, 120),
                Province = TrimOrNull(form.Province, 80),
                SearchText = TrimOrNull(form.SearchText, 300),
                SessionId = TrimOrNull(form.SessionId, 120),
                OccurredAt = form.OccurredAt ?? DateTime.UtcNow,
                CreatorId = userId
            });

            await _context.SaveChangesAsync();
            return Ok(new { tracked = true }, "تم تسجيل الحدث بنجاح.");
        }

        public async Task<IActionResult> GetAdminSummaryAsync(DateOnly? fromDate, DateOnly? toDate)
        {
            var (from, to) = ResolveRange(fromDate, toDate);
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var weekStart = today.AddDays(-7);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var soon = today.AddDays(14);

            var events = await Events(from, to).ToListAsync();
            var appointments = await _context.Appointments
                .AsNoTracking()
                .Where(a => !a.IsDeleted)
                .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
                .Include(a => a.Clinic)
                .ToListAsync();
            var doctors = await _context.Doctors
                .AsNoTracking()
                .Where(d => !d.IsDeleted)
                .Include(d => d.Specialization)
                .ToListAsync();
            var users = await _context.AspNetUsers
                .AsNoTracking()
                .Where(u => !u.IsDeleted)
                .ToListAsync();
            var subscriptions = await _context.DoctorSubscriptions
                .AsNoTracking()
                .Where(s => !s.IsDeleted)
                .Include(s => s.Package)
                .ToListAsync();

            var activeSubscriptions = subscriptions
                .Where(s => s.Status == SubscriptionStatus.Active && s.EndDate.Date >= today)
                .ToList();
            var appointmentsInRange = appointments
                .Where(a => a.AppointmentDate >= from && a.AppointmentDate < to)
                .ToList();
            var profileViews = Count(events, EventNames.ProfileViewed);
            var searchAppearances = Count(events, EventNames.DoctorShownInSearch);
            var bookingClicks = Count(events, EventNames.BookingClicked);
            var bookingsFromEvents = Count(events, EventNames.AppointmentCreated);

            var summary = new AnalyticsSummaryDto
            {
                Metrics =
                {
                    Metric("usersTotal", "إجمالي المستخدمين", users.Count),
                    Metric("usersInRange", "مستخدمين جدد بالفترة", users.Count(u => IsWithin(u.CreatedAt, from, to))),
                    Metric("usersToday", "مستخدمين جدد اليوم", users.Count(u => IsWithin(u.CreatedAt, today, tomorrow))),
                    Metric("usersThisWeek", "مستخدمين جدد هذا الأسبوع", users.Count(u => IsWithin(u.CreatedAt, weekStart, tomorrow))),
                    Metric("usersThisMonth", "مستخدمين جدد هذا الشهر", users.Count(u => IsWithin(u.CreatedAt, monthStart, tomorrow))),
                    Metric("doctorsTotal", "إجمالي الأطباء", doctors.Count),
                    Metric("visibleDoctors", "أطباء ظاهرين للعامة", doctors.Count(d => d.IsPubliclyVisible)),
                    Metric("hiddenDoctors", "أطباء غير ظاهرين", doctors.Count(d => !d.IsPubliclyVisible)),
                    Metric("appointmentsTotal", "إجمالي الحجوزات", appointments.Count),
                    Metric("appointmentsInRange", "حجوزات الفترة", appointmentsInRange.Count),
                    Metric("appointmentsToday", "حجوزات اليوم", appointments.Count(a => a.AppointmentDate.Date == today)),
                    Metric("appointmentsThisMonth", "حجوزات هذا الشهر", appointments.Count(a => a.AppointmentDate.Date >= monthStart && a.AppointmentDate.Date < tomorrow)),
                    Metric("searches", "عدد عمليات البحث", Count(events, EventNames.SearchPerformed)),
                    Metric("profileViews", "فتح البروفايلات", profileViews),
                    Metric("bookingClicks", "ضغطات الحجز", bookingClicks),
                    Metric("createdBookingsFromEvents", "حجوزات مسجلة من الأحداث", bookingsFromEvents),
                    Metric("dailyActiveUsers", "مستخدمون نشطون يومياً", ActiveUsers(events.Where(e => e.OccurredAt.Date == today))),
                    Metric("monthlyActiveUsers", "مستخدمون نشطون شهرياً", ActiveUsers(events.Where(e => e.OccurredAt.Date >= monthStart)))
                },
                AppointmentStatus = AppointmentStatuses(appointmentsInRange),
                AppointmentSources = AppointmentSources(appointmentsInRange),
                TopDoctorsByViews = TopDoctorsByEvents(events, doctors, EventNames.ProfileViewed),
                TopDoctorsByBookings = TopDoctorsByBookings(appointmentsInRange),
                TopSpecializationsBySearch = TopSpecializationsBySearch(events, doctors),
                TopSpecializationsByBookings = TopSpecializationsByBookings(appointmentsInRange),
                TopProvinces = TopProvinces(events, appointmentsInRange),
                TopSearchTerms = TopSearchTerms(events),
                TopPages = TopPages(events),
                RecentEvents = RecentEvents(events),
                UserGrowth = Trend(users.Select(u => u.CreatedAt ?? DateTime.MinValue), from, to),
                AppointmentTrend = Trend(appointmentsInRange.Select(a => a.AppointmentDate), from, to),
                Offers = new OfferAnalyticsDto
                {
                    Views = Count(events, EventNames.OfferViewed),
                    Clicks = Count(events, EventNames.OfferClicked),
                    BookingsFromOffers = Count(events, EventNames.AppointmentCreated, "offer")
                },
                Subscriptions = new SubscriptionAnalyticsDto
                {
                    ActiveSubscribers = activeSubscriptions.Select(s => s.DoctorId).Distinct().Count(),
                    PremiumSubscribers = activeSubscriptions.Count(s => IsPremium(s.Package?.NormalizedName)),
                    BasicSubscribers = activeSubscriptions.Count(s => !IsPremium(s.Package?.NormalizedName)),
                    ExpiredSubscriptions = subscriptions.Count(s => s.Status == SubscriptionStatus.Expired || s.EndDate.Date < today),
                    ExpiringSoon = activeSubscriptions.Count(s => s.EndDate.Date <= soon)
                },
                Conversions = new ConversionAnalyticsDto
                {
                    SearchToProfileRate = Rate(profileViews, searchAppearances),
                    ProfileToBookingRate = Rate(bookingsFromEvents, profileViews)
                }
            };

            return Ok(summary, "تم جلب إحصائيات الأدمن بنجاح.");
        }

        public async Task<IActionResult> GetDoctorSummaryAsync(DateOnly? fromDate, DateOnly? toDate)
        {
            var userId = _load.GetCurrentUserId();
            var doctor = await _context.Doctors
                .AsNoTracking()
                .FirstOrDefaultAsync(d => !d.IsDeleted && d.UserId == userId);

            if (doctor == null)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "لم يتم العثور على ملف الطبيب.",
                    Data = null
                });
            }

            return await BuildDoctorSummaryAsync(doctor.Id, fromDate, toDate);
        }

        public async Task<IActionResult> GetDoctorSummaryForAdminAsync(int doctorId, DateOnly? fromDate, DateOnly? toDate)
        {
            var exists = await _context.Doctors
                .AsNoTracking()
                .AnyAsync(d => !d.IsDeleted && d.Id == doctorId);
            if (!exists)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "لم يتم العثور على الطبيب.",
                    Data = null
                });
            }

            return await BuildDoctorSummaryAsync(doctorId, fromDate, toDate);
        }

        private async Task<IActionResult> BuildDoctorSummaryAsync(int doctorId, DateOnly? fromDate, DateOnly? toDate)
        {
            var (from, to) = ResolveRange(fromDate, toDate);
            var today = DateTime.Today;
            var weekEnd = today.AddDays(7);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var events = await Events(from, to)
                .Where(e => e.DoctorId == doctorId)
                .ToListAsync();
            var appointments = await _context.Appointments
                .AsNoTracking()
                .Where(a => !a.IsDeleted && a.DoctorId == doctorId)
                .Include(a => a.Clinic)
                .ToListAsync();
            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r => !r.IsDeleted && r.DoctorId == doctorId)
                .ToListAsync();

            var searchAppearances = Count(events, EventNames.DoctorShownInSearch);
            var profileViews = Count(events, EventNames.ProfileViewed);
            var bookingClicks = Count(events, EventNames.BookingClicked);
            var bookingsFromEvents = Count(events, EventNames.AppointmentCreated);
            var appointmentsInRange = appointments
                .Where(a => a.AppointmentDate >= from && a.AppointmentDate < to)
                .ToList();

            var summary = new AnalyticsSummaryDto
            {
                Metrics =
                {
                    Metric("searchAppearances", "ظهور الطبيب بنتائج البحث", searchAppearances),
                    Metric("profileViews", "فتح بروفايل الطبيب", profileViews),
                    Metric("bookingClicks", "ضغط زر الحجز", bookingClicks),
                    Metric("createdBookingsFromEvents", "حجوزات من التفاعل", bookingsFromEvents),
                    Metric("completedBookings", "حجوزات مكتملة بالفترة", appointmentsInRange.Count(a => a.Status == AppointmentStatus.Completed)),
                    Metric("cancelledBookings", "حجوزات ملغية بالفترة", appointmentsInRange.Count(a => a.Status == AppointmentStatus.Cancelled)),
                    Metric("upcomingBookings", "حجوزات قادمة", appointments.Count(a => a.AppointmentDate.Date >= today && a.Status != AppointmentStatus.Cancelled)),
                    Metric("todayBookings", "حجوزات اليوم", appointments.Count(a => a.AppointmentDate.Date == today)),
                    Metric("weekBookings", "حجوزات هذا الأسبوع", appointments.Count(a => a.AppointmentDate.Date >= today && a.AppointmentDate.Date <= weekEnd)),
                    Metric("monthBookings", "حجوزات هذا الشهر", appointments.Count(a => a.AppointmentDate.Date >= monthStart && a.AppointmentDate.Date < monthEnd)),
                    Metric("reviewCount", "عدد التقييمات", reviews.Count),
                    Metric("averageRating", "متوسط التقييم", reviews.Count == 0 ? 0 : Math.Round((decimal)reviews.Average(r => r.Rating), 2)),
                    Metric("offerViews", "مشاهدات عروض الطبيب", Count(events, EventNames.OfferViewed)),
                    Metric("offerClicks", "ضغطات عروض الطبيب", Count(events, EventNames.OfferClicked))
                },
                AppointmentStatus = AppointmentStatuses(appointmentsInRange),
                AppointmentSources = AppointmentSources(appointmentsInRange),
                TopClinicsByBookings = appointmentsInRange
                    .GroupBy(a => a.Clinic?.Name ?? "عيادة غير معروفة")
                    .OrderByDescending(g => g.Count())
                    .Take(8)
                    .Select(g => Item(g.Key, g.Count()))
                    .ToList(),
                TopBookingDays = appointmentsInRange
                    .GroupBy(a => ArabicDay(a.AppointmentDate.DayOfWeek))
                    .OrderByDescending(g => g.Count())
                    .Select(g => Item(g.Key, g.Count()))
                    .ToList(),
                TopProvinces = TopProvinces(events, appointmentsInRange),
                PeakBookingHours = PeakBookingHours(appointmentsInRange),
                RecentEvents = RecentEvents(events),
                AppointmentTrend = Trend(appointmentsInRange.Select(a => a.AppointmentDate), from, to),
                Offers = new OfferAnalyticsDto
                {
                    Views = Count(events, EventNames.OfferViewed),
                    Clicks = Count(events, EventNames.OfferClicked),
                    BookingsFromOffers = Count(events, EventNames.AppointmentCreated, "offer")
                },
                Conversions = new ConversionAnalyticsDto
                {
                    SearchToProfileRate = Rate(profileViews, searchAppearances),
                    ProfileToBookingRate = Rate(bookingsFromEvents, profileViews)
                }
            };

            return Ok(summary, "تم جلب إحصائيات الطبيب بنجاح.");
        }

        private IQueryable<AnalyticsEvent> Events(DateTime from, DateTime to) =>
            _context.AnalyticsEvents
                .AsNoTracking()
                .Where(e => !e.IsDeleted && e.OccurredAt >= from && e.OccurredAt < to);

        private OkObjectResult Ok<T>(T data, string message) => new(new ResponseDto<T>
        {
            Status = "Success",
            Code = 200,
            Message = message,
            Data = data
        });

        private static BadRequestObjectResult Error(string message) => new(new ResponseDto<object>
        {
            Status = "Error",
            Code = 400,
            Message = message,
            Data = null
        });

        private static (DateTime From, DateTime To) ResolveRange(DateOnly? fromDate, DateOnly? toDate)
        {
            var to = (toDate ?? DateOnly.FromDateTime(DateTime.Today)).ToDateTime(TimeOnly.MinValue).AddDays(1);
            var from = (fromDate ?? DateOnly.FromDateTime(to.AddDays(-30))).ToDateTime(TimeOnly.MinValue);
            return from >= to ? (to.AddDays(-30), to) : (from, to);
        }

        private static string? TrimOrNull(string? value, int max)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var trimmed = value.Trim();
            return trimmed.Length <= max ? trimmed : trimmed[..max];
        }

        private static MetricDto Metric(string key, string label, decimal value, string? note = null) =>
            new() { Key = key, Label = label, Value = value, Note = note };

        private static LabelValueDto Item(string label, decimal value) =>
            new() { Label = label, Value = value };

        private static int Count(IEnumerable<AnalyticsEvent> events, string eventType) =>
            events.Count(e => e.EventType == eventType);

        private static int Count(IEnumerable<AnalyticsEvent> events, string eventType, string source) =>
            events.Count(e => e.EventType == eventType && string.Equals(e.Source, source, StringComparison.OrdinalIgnoreCase));

        private static decimal Rate(decimal value, decimal total) =>
            total <= 0 ? 0 : Math.Round(value / total * 100, 2);

        private static bool IsWithin(DateTime? value, DateTime from, DateTime to) =>
            value.HasValue && value.Value >= from && value.Value < to;

        private static bool IsPremium(string? packageName)
        {
            var normalized = (packageName ?? string.Empty).Trim().ToLowerInvariant();
            return normalized.Contains("premium") || normalized.Contains("diamond") || normalized.Contains("gold");
        }

        private static int ActiveUsers(IEnumerable<AnalyticsEvent> events) =>
            events.Select(e => e.UserId?.ToString() ?? e.SessionId ?? $"guest-{e.Id}")
                .Distinct()
                .Count();

        private static List<LabelValueDto> AppointmentStatuses(IEnumerable<Entities.Appointment.Appointment> appointments) =>
            new()
            {
                Item("قيد الانتظار", appointments.Count(a => a.Status == AppointmentStatus.Pending)),
                Item("مؤكدة", appointments.Count(a => a.Status == AppointmentStatus.Confirmed)),
                Item("ملغية", appointments.Count(a => a.Status == AppointmentStatus.Cancelled)),
                Item("مكتملة", appointments.Count(a => a.Status == AppointmentStatus.Completed)),
            };

        private static List<LabelValueDto> AppointmentSources(IEnumerable<Entities.Appointment.Appointment> appointments) =>
            new()
            {
                Item("زائر", appointments.Count(a => !a.UserId.HasValue && !a.CreatorId.HasValue)),
                Item("حساب مسجل", appointments.Count(a => a.UserId.HasValue)),
                Item("حجز يدوي", appointments.Count(a => !a.UserId.HasValue && a.CreatorId.HasValue)),
            };

        private static List<LabelValueDto> TopDoctorsByEvents(
            List<AnalyticsEvent> events,
            List<Entities.Doctor.Doctor> doctors,
            string eventType) =>
            events.Where(e => e.EventType == eventType && e.DoctorId.HasValue)
                .GroupBy(e => e.DoctorId!.Value)
                .OrderByDescending(g => g.Count())
                .Take(8)
                .Select(g => Item(doctors.FirstOrDefault(d => d.Id == g.Key)?.Name ?? $"طبيب #{g.Key}", g.Count()))
                .ToList();

        private static List<LabelValueDto> TopDoctorsByBookings(List<Entities.Appointment.Appointment> appointments) =>
            appointments.GroupBy(a => a.Doctor?.Name ?? $"طبيب #{a.DoctorId}")
                .OrderByDescending(g => g.Count())
                .Take(8)
                .Select(g => Item(g.Key, g.Count()))
                .ToList();

        private static List<LabelValueDto> TopSpecializationsBySearch(
            List<AnalyticsEvent> events,
            List<Entities.Doctor.Doctor> doctors) =>
            events.Where(e => e.EventType == EventNames.SearchPerformed && e.SpecializationId.HasValue)
                .GroupBy(e => e.SpecializationId!.Value)
                .OrderByDescending(g => g.Count())
                .Take(8)
                .Select(g => Item(doctors.FirstOrDefault(d => d.SpecializationId == g.Key)?.Specialization?.Name ?? $"اختصاص #{g.Key}", g.Count()))
                .ToList();

        private static List<LabelValueDto> TopSpecializationsByBookings(List<Entities.Appointment.Appointment> appointments) =>
            appointments.GroupBy(a => a.Doctor?.Specialization?.Name ?? "غير محدد")
                .OrderByDescending(g => g.Count())
                .Take(8)
                .Select(g => Item(g.Key, g.Count()))
                .ToList();

        private static List<LabelValueDto> TopProvinces(
            List<AnalyticsEvent> events,
            List<Entities.Appointment.Appointment> appointments)
        {
            var eventItems = events.Where(e => !string.IsNullOrWhiteSpace(e.Province)).Select(e => e.Province!);
            var appointmentItems = appointments
                .Where(a => a.Clinic != null)
                .Select(a => a.Clinic!.IraqiProvince.GetDisplayName());

            return eventItems.Concat(appointmentItems)
                .GroupBy(x => x)
                .OrderByDescending(g => g.Count())
                .Take(8)
                .Select(g => Item(g.Key, g.Count()))
                .ToList();
        }

        private static List<LabelValueDto> TopSearchTerms(List<AnalyticsEvent> events) =>
            events.Where(e => e.EventType == EventNames.SearchPerformed && !string.IsNullOrWhiteSpace(e.SearchText))
                .GroupBy(e => e.SearchText!)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => Item(g.Key, g.Count()))
                .ToList();

        private static List<LabelValueDto> TopPages(List<AnalyticsEvent> events) =>
            events.Where(e => e.EventType == EventNames.PageViewed && !string.IsNullOrWhiteSpace(e.Page))
                .GroupBy(e => e.Page!)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => Item(g.Key, g.Count()))
                .ToList();

        private static List<LabelValueDto> PeakBookingHours(IEnumerable<Entities.Appointment.Appointment> appointments) =>
            appointments.GroupBy(a => $"{a.CreatedAt?.Hour ?? 0:00}:00")
                .OrderByDescending(g => g.Count())
                .Take(8)
                .Select(g => Item(g.Key, g.Count()))
                .ToList();

        private static List<TrendPointDto> Trend(IEnumerable<DateTime> dates, DateTime from, DateTime to)
        {
            var grouped = dates
                .Where(d => d >= from && d < to)
                .GroupBy(d => d.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = new List<TrendPointDto>();
            for (var day = from.Date; day < to.Date; day = day.AddDays(1))
            {
                result.Add(new TrendPointDto
                {
                    Date = day,
                    Label = day.ToString("MM-dd"),
                    Value = grouped.GetValueOrDefault(day)
                });
            }
            return result;
        }

        private static List<AnalyticsEventItemDto> RecentEvents(List<AnalyticsEvent> events) =>
            events.OrderByDescending(e => e.OccurredAt)
                .Take(12)
                .Select(e => new AnalyticsEventItemDto
                {
                    EventType = e.EventType,
                    Label = EventLabel(e.EventType),
                    OccurredAt = e.OccurredAt,
                    Source = e.Source,
                    Page = e.Page,
                    DoctorId = e.DoctorId,
                    OfferId = e.OfferId
                })
                .ToList();

        private static string EventLabel(string eventType) => eventType switch
        {
            EventNames.SearchPerformed => "بحث عن طبيب",
            EventNames.DoctorShownInSearch => "ظهور طبيب بالبحث",
            EventNames.ProfileViewed => "فتح بروفايل",
            EventNames.BookingClicked => "ضغط زر الحجز",
            EventNames.AppointmentCreated => "إنشاء حجز",
            EventNames.OfferViewed => "مشاهدة عرض",
            EventNames.OfferClicked => "ضغط عرض",
            EventNames.PageViewed => "فتح صفحة",
            _ => eventType
        };

        private static string ArabicDay(DayOfWeek day) => day switch
        {
            DayOfWeek.Saturday => "السبت",
            DayOfWeek.Sunday => "الأحد",
            DayOfWeek.Monday => "الاثنين",
            DayOfWeek.Tuesday => "الثلاثاء",
            DayOfWeek.Wednesday => "الأربعاء",
            DayOfWeek.Thursday => "الخميس",
            DayOfWeek.Friday => "الجمعة",
            _ => day.ToString()
        };

        private static class EventNames
        {
            public const string SearchPerformed = "doctor_search_performed";
            public const string DoctorShownInSearch = "doctor_shown_in_search";
            public const string ProfileViewed = "doctor_profile_viewed";
            public const string BookingClicked = "doctor_booking_clicked";
            public const string AppointmentCreated = "appointment_created";
            public const string OfferViewed = "offer_viewed";
            public const string OfferClicked = "offer_clicked";
            public const string PageViewed = "page_viewed";
        }
    }
}
