using Clinic_Booking.DTOs.DoctorOfferDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.DoctorOffer;
using Clinic_Booking.Enums;
using Clinic_Booking.Extensions;
using Clinic_Booking.IServices.IDoctorOfferServices;
using Clinic_Booking.IServices.ILoadServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.DoctorOfferServices
{
    public class DoctorOfferServices : IDoctorOfferServices
    {
        private const int MaxOfferDurationDays = 7;
        private readonly Data.ApplicationDbContext _context;
        private readonly ILoadServices _load;

        public DoctorOfferServices(Data.ApplicationDbContext context, ILoadServices load)
        {
            _context = context;
            _load = load;
        }

        public async Task<ActionResult<PaginationDto.PageResult<DoctorOfferDto>>> GetListAsync(
            SearchDoctorOfferDto filter,
            int page = 1,
            int pageSize = 10)
        {
            return await GetPagedAsync(filter ?? new SearchDoctorOfferDto(), page, pageSize, null);
        }

        public async Task<ActionResult<PaginationDto.PageResult<DoctorOfferDto>>> GetMineAsync(
            SearchDoctorOfferDto filter,
            int page = 1,
            int pageSize = 10)
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return DoctorProfileNotFoundPage();

            filter ??= new SearchDoctorOfferDto();
            filter.DoctorId = doctor.Id;
            return await GetPagedAsync(filter, page, pageSize, doctor.Id);
        }

        public async Task<ActionResult<PaginationDto.PageResult<DoctorOfferDto>>> GetPublicAsync(
            SearchDoctorOfferDto filter,
            int page = 1,
            int pageSize = 10)
        {
            filter ??= new SearchDoctorOfferDto();
            filter.IsActive = true;
            filter.CurrentlyVisible = true;
            return await GetPagedAsync(filter, page, Math.Min(pageSize, 30), null);
        }

        public async Task<IActionResult> GetQuotaAsync(int doctorId)
        {
            var quota = await BuildQuotaAsync(doctorId);
            if (quota == null) return DoctorNotFound();
            return Success("تم جلب حدود العروض بنجاح.", quota);
        }

        public async Task<IActionResult> GetMyQuotaAsync()
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return DoctorProfileNotFound();

            var quota = await BuildQuotaAsync(doctor.Id);
            return Success("تم جلب حدود العروض بنجاح.", quota);
        }

        public async Task<IActionResult> AddAsync(DoctorOfferUpsertDto form)
        {
            if (form.DoctorId is null or <= 0) return BadRequest("يجب اختيار الطبيب.");
            return await SaveAsync(form, form.DoctorId.Value, null);
        }

        public async Task<IActionResult> AddMineAsync(DoctorOfferUpsertDto form)
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return DoctorProfileNotFound();

            return await SaveAsync(form, doctor.Id, doctor.Id);
        }

        public async Task<IActionResult> UpdateAsync(DoctorOfferUpsertDto form)
        {
            if (form.Id is null or <= 0) return BadRequest("معرف العرض مطلوب.");

            var offer = await _context.DoctorOffers.FirstOrDefaultAsync(item => item.Id == form.Id && !item.IsDeleted);
            if (offer == null) return NotFound("العرض غير موجود.");

            var doctorId = form.DoctorId is > 0 ? form.DoctorId.Value : offer.DoctorId;
            return await SaveAsync(form, doctorId, null, offer);
        }

        public async Task<IActionResult> UpdateMineAsync(DoctorOfferUpsertDto form)
        {
            if (form.Id is null or <= 0) return BadRequest("معرف العرض مطلوب.");

            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return DoctorProfileNotFound();

            var offer = await _context.DoctorOffers.FirstOrDefaultAsync(item =>
                item.Id == form.Id && item.DoctorId == doctor.Id && !item.IsDeleted);
            if (offer == null) return NotFound("العرض غير موجود ضمن حسابك.");

            return await SaveAsync(form, doctor.Id, doctor.Id, offer);
        }

        public async Task<IActionResult> DeleteAsync(int id)
        {
            return await DeleteInternalAsync(id, null);
        }

        public async Task<IActionResult> DeleteMineAsync(int id)
        {
            var doctor = await GetCurrentDoctorAsync();
            if (doctor == null) return DoctorProfileNotFound();

            return await DeleteInternalAsync(id, doctor.Id);
        }

        private async Task<ActionResult<PaginationDto.PageResult<DoctorOfferDto>>> GetPagedAsync(
            SearchDoctorOfferDto filter,
            int page,
            int pageSize,
            int? forcedDoctorId)
        {
            if (page <= 0 || pageSize <= 0)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "قيم الترقيم غير صحيحة."
                });
            }

            var now = DateTime.UtcNow;
            var query = _context.DoctorOffers
                .AsNoTracking()
                .Include(item => item.Doctor)
                    .ThenInclude(doctor => doctor.DoctorSubscriptions)
                    .ThenInclude(subscription => subscription.Package)
                .Include(item => item.Clinic)
                .Where(item => !item.IsDeleted)
                .Where(item => forcedDoctorId == null || item.DoctorId == forcedDoctorId)
                .Where(item => filter.DoctorId == null || item.DoctorId == filter.DoctorId)
                .Where(item => filter.ClinicId == null || item.ClinicId == filter.ClinicId)
                .Where(item => filter.IsActive == null || item.IsActive == filter.IsActive)
                .Where(item => filter.CurrentlyVisible == null ||
                    (filter.CurrentlyVisible == true
                        ? item.IsActive && item.StartsAt <= now && item.EndsAt >= now
                        : !(item.IsActive && item.StartsAt <= now && item.EndsAt >= now)));

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.Trim();
                query = query.Where(item =>
                    item.Title.Contains(term) ||
                    (item.Description != null && item.Description.Contains(term)) ||
                    item.Doctor.Name.Contains(term) ||
                    (item.Clinic != null && item.Clinic.Name.Contains(term)));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var offers = await query
                .OrderByDescending(item => item.IsActive && item.StartsAt <= now && item.EndsAt >= now)
                .ThenByDescending(item => item.Doctor.DoctorSubscriptions
                    .Where(subscription =>
                        subscription.Status == SubscriptionStatus.Active &&
                        subscription.StartDate <= now &&
                        subscription.EndDate >= now)
                    .Select(subscription => (decimal?)subscription.Package.YearlyPrice)
                    .Max() ?? 0)
                .ThenByDescending(item => item.Doctor.DoctorSubscriptions
                    .Where(subscription =>
                        subscription.Status == SubscriptionStatus.Active &&
                        subscription.StartDate <= now &&
                        subscription.EndDate >= now)
                    .Select(subscription => (decimal?)subscription.Package.Price)
                    .Max() ?? 0)
                .ThenBy(item => item.EndsAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = offers.Select(item => ToDto(item, now)).ToList();
            return new OkObjectResult(new ResponseDto<PaginationDto.PageResult<DoctorOfferDto>>
            {
                Status = "Success",
                Code = 200,
                Message = "تم جلب العروض بنجاح.",
                Data = new PaginationDto.PageResult<DoctorOfferDto>(items, totalItems, totalPages, page, pageSize)
            });
        }

        private async Task<IActionResult> SaveAsync(
            DoctorOfferUpsertDto form,
            int doctorId,
            int? forcedDoctorId,
            DoctorOffer? existing = null)
        {
            var validation = await ValidateAsync(form, doctorId, forcedDoctorId, existing?.Id);
            if (validation != null) return validation;

            var userId = _load.GetCurrentUserId();
            var offer = existing ?? new DoctorOffer
            {
                DoctorId = doctorId,
                CreatorId = userId,
                CreatedAt = DateTime.UtcNow
            };

            offer.DoctorId = doctorId;
            offer.ClinicId = form.AppliesToAllClinics ? null : form.ClinicId;
            offer.Title = form.Title.Trim();
            offer.Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim();
            offer.OfferType = form.OfferType;
            offer.OriginalPrice = form.OriginalPrice;
            offer.OfferPrice = form.OfferPrice;
            offer.DiscountPercent = form.DiscountPercent;
            offer.BadgeText = string.IsNullOrWhiteSpace(form.BadgeText) ? null : form.BadgeText.Trim();
            offer.Terms = string.IsNullOrWhiteSpace(form.Terms) ? null : form.Terms.Trim();
            offer.StartsAt = NormalizeUtc(form.StartsAt);
            offer.EndsAt = NormalizeUtc(form.EndsAt);
            offer.IsActive = form.IsActive;
            offer.ModifierId = userId;
            offer.ModifiedAt = DateTime.UtcNow;

            if (existing == null) _context.DoctorOffers.Add(offer);

            await _context.SaveChangesAsync();
            return Success(existing == null ? "تم إنشاء العرض بنجاح." : "تم تحديث العرض بنجاح.", true);
        }

        private async Task<IActionResult?> ValidateAsync(
            DoctorOfferUpsertDto form,
            int doctorId,
            int? forcedDoctorId,
            int? existingOfferId)
        {
            if (forcedDoctorId != null && doctorId != forcedDoctorId)
            {
                return BadRequest("لا يمكنك إدارة عروض طبيب آخر.");
            }

            var doctorExists = await _context.Doctors.AnyAsync(doctor => doctor.Id == doctorId && !doctor.IsDeleted);
            if (!doctorExists) return DoctorNotFound();

            if (string.IsNullOrWhiteSpace(form.Title) || form.Title.Trim().Length < 3)
            {
                return BadRequest("عنوان العرض يجب أن لا يقل عن 3 أحرف.");
            }

            if (form.Title.Trim().Length > 160) return BadRequest("عنوان العرض طويل جداً.");
            if (form.Description?.Length > 800) return BadRequest("وصف العرض يجب أن لا يتجاوز 800 حرف.");
            if (form.Terms?.Length > 600) return BadRequest("شروط العرض يجب أن لا تتجاوز 600 حرف.");
            if (form.BadgeText?.Length > 40) return BadRequest("شارة العرض يجب أن لا تتجاوز 40 حرفاً.");

            var startsAt = NormalizeUtc(form.StartsAt);
            var endsAt = NormalizeUtc(form.EndsAt);
            if (endsAt <= startsAt) return BadRequest("تاريخ نهاية العرض يجب أن يكون بعد تاريخ البداية.");
            if ((endsAt - startsAt).TotalDays > MaxOfferDurationDays)
            {
                return BadRequest("مدة ظهور العرض لا يمكن أن تتجاوز 7 أيام.");
            }

            if (!form.AppliesToAllClinics)
            {
                if (form.ClinicId is null or <= 0) return BadRequest("اختر عيادة أو خيار جميع العيادات.");

                var clinicBelongsToDoctor = await _context.Clinics.AnyAsync(clinic =>
                    clinic.Id == form.ClinicId && clinic.DoctorId == doctorId && !clinic.IsDeleted);
                if (!clinicBelongsToDoctor) return BadRequest("العيادة المختارة غير تابعة لهذا الطبيب.");
            }

            var typeValidation = ValidateOfferType(form);
            if (typeValidation != null) return typeValidation;

            var quota = await BuildQuotaAsync(doctorId);
            if (quota == null) return DoctorNotFound();
            if (!quota.CanMakeOffers) return BadRequest("باقة الطبيب الحالية لا تسمح بإنشاء العروض.");

            var becomesActive = form.IsActive && endsAt >= DateTime.UtcNow;
            if (becomesActive)
            {
                var activeOffers = await ActiveOfferCountAsync(doctorId, existingOfferId);
                if (activeOffers >= quota.MaxActiveOffers)
                {
                    return BadRequest($"وصل الطبيب إلى الحد الأعلى للعروض الفعالة: {quota.MaxActiveOffers}.");
                }
            }

            return null;
        }

        private static IActionResult? ValidateOfferType(DoctorOfferUpsertDto form)
        {
            if (form.OriginalPrice is <= 0) return BadRequest("السعر قبل العرض يجب أن يكون أكبر من صفر.");
            if (form.OfferPrice is <= 0) return BadRequest("سعر العرض يجب أن يكون أكبر من صفر.");

            return form.OfferType switch
            {
                DoctorOfferType.PercentageDiscount when form.DiscountPercent is null or <= 0 or > 100
                    => BadRequest("نسبة الخصم يجب أن تكون بين 1 و 100."),
                DoctorOfferType.FixedPrice when form.OfferPrice is null or <= 0
                    => BadRequest("السعر الخاص مطلوب ويجب أن يكون أكبر من صفر."),
                DoctorOfferType.FixedPrice when form.OriginalPrice != null && form.OriginalPrice <= form.OfferPrice
                    => BadRequest("السعر قبل العرض يجب أن يكون أكبر من سعر العرض."),
                DoctorOfferType.ServicePackage when form.OfferPrice is null or <= 0
                    => BadRequest("سعر الباقة مطلوب ويجب أن يكون أكبر من صفر."),
                _ => null
            };
        }

        private async Task<IActionResult> DeleteInternalAsync(int id, int? forcedDoctorId)
        {
            var offer = await _context.DoctorOffers.FirstOrDefaultAsync(item =>
                item.Id == id && !item.IsDeleted && (forcedDoctorId == null || item.DoctorId == forcedDoctorId));
            if (offer == null) return NotFound("العرض غير موجود.");

            offer.IsDeleted = true;
            offer.DeletedAt = DateTime.UtcNow;
            offer.DeleterId = _load.GetCurrentUserId();
            await _context.SaveChangesAsync();
            return Success("تم حذف العرض بنجاح.", true);
        }

        private async Task<DoctorOfferQuotaDto?> BuildQuotaAsync(int doctorId)
        {
            var now = DateTime.UtcNow;
            var doctorExists = await _context.Doctors.AnyAsync(doctor => doctor.Id == doctorId && !doctor.IsDeleted);
            if (!doctorExists) return null;

            var package = await _context.DoctorSubscriptions
                .Where(subscription =>
                    subscription.DoctorId == doctorId &&
                    subscription.Status == SubscriptionStatus.Active &&
                    subscription.StartDate <= now &&
                    subscription.EndDate >= now)
                .OrderByDescending(subscription => subscription.Package.MaxActiveOffers)
                .Select(subscription => new
                {
                    subscription.Package.Name,
                    subscription.Package.MakeOffers,
                    subscription.Package.MaxActiveOffers
                })
                .FirstOrDefaultAsync();

            var activeOffers = await ActiveOfferCountAsync(doctorId, null);
            var maxOffers = package?.MaxActiveOffers ?? 0;
            return new DoctorOfferQuotaDto
            {
                DoctorId = doctorId,
                CanMakeOffers = package?.MakeOffers == true && maxOffers > 0,
                MaxActiveOffers = maxOffers,
                ActiveOffers = activeOffers,
                RemainingOffers = Math.Max(0, maxOffers - activeOffers),
                PackageName = package?.Name
            };
        }

        private async Task<int> ActiveOfferCountAsync(int doctorId, int? exceptOfferId)
        {
            var now = DateTime.UtcNow;
            return await _context.DoctorOffers.CountAsync(offer =>
                offer.DoctorId == doctorId &&
                offer.IsActive &&
                !offer.IsDeleted &&
                offer.EndsAt >= now &&
                (exceptOfferId == null || offer.Id != exceptOfferId));
        }

        private async Task<Entities.Doctor.Doctor?> GetCurrentDoctorAsync()
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty) return null;

            return await _context.Doctors.FirstOrDefaultAsync(doctor => doctor.UserId == userId && !doctor.IsDeleted);
        }

        private static DoctorOfferDto ToDto(DoctorOffer item, DateTime now)
        {
            var durationDays = Math.Max(1, (int)Math.Ceiling((item.EndsAt - item.StartsAt).TotalDays));
            var activeSubscription = item.Doctor.DoctorSubscriptions
                .Where(subscription =>
                    subscription.Status == SubscriptionStatus.Active &&
                    subscription.StartDate <= now &&
                    subscription.EndDate >= now)
                .OrderByDescending(subscription => subscription.Package.YearlyPrice)
                .ThenByDescending(subscription => subscription.Package.Price)
                .FirstOrDefault();

            return new DoctorOfferDto
            {
                Id = item.Id,
                DoctorId = item.DoctorId,
                DoctorName = item.Doctor.Name,
                IsFeatured = string.Equals(
                    activeSubscription?.Package.NormalizedName,
                    "Premium",
                    StringComparison.OrdinalIgnoreCase),
                ActiveSubscriptionName = activeSubscription?.Package.Name,
                ActiveSubscriptionNormalizedName = activeSubscription?.Package.NormalizedName,
                ActiveSubscriptionWeight = activeSubscription?.Package.YearlyPrice ?? 0,
                ClinicId = item.ClinicId,
                ClinicName = item.Clinic?.Name,
                AppliesToAllClinics = item.ClinicId == null,
                Title = item.Title,
                Description = item.Description,
                OfferType = item.OfferType,
                OfferTypeName = item.OfferType.GetDisplayName(),
                OriginalPrice = item.OriginalPrice,
                OfferPrice = item.OfferPrice,
                DiscountPercent = item.DiscountPercent,
                BadgeText = item.BadgeText,
                Terms = item.Terms,
                StartsAt = item.StartsAt,
                EndsAt = item.EndsAt,
                DurationDays = durationDays,
                RemainingDays = Math.Max(0, (int)Math.Ceiling((item.EndsAt - now).TotalDays)),
                IsActive = item.IsActive,
                IsCurrentlyVisible = item.IsActive && item.StartsAt <= now && item.EndsAt >= now
            };
        }

        private static DateTime NormalizeUtc(DateTime value)
        {
            return value.Kind == DateTimeKind.Utc
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime();
        }

        private static IActionResult Success<T>(string message, T data)
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
                Status = "Not Found",
                Code = 404,
                Message = message
            });
        }

        private static IActionResult DoctorNotFound() => NotFound("الطبيب غير موجود.");
        private static IActionResult DoctorProfileNotFound() => NotFound("لا يوجد ملف طبيب مرتبط بحسابك.");
        private static ActionResult<PaginationDto.PageResult<DoctorOfferDto>> DoctorProfileNotFoundPage()
        {
            return new NotFoundObjectResult(new ResponseDto<string>
            {
                Status = "Not Found",
                Code = 404,
                Message = "لا يوجد ملف طبيب مرتبط بحسابك."
            });
        }
    }
}
