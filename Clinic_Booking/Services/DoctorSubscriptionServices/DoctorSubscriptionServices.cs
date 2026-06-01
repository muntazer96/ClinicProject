using Clinic_Booking.Data;
using Clinic_Booking.DTOs.DoctorDTO;
using Clinic_Booking.DTOs.DoctorSubscriptionDTO;
using Clinic_Booking.DTOs.SubscriptionPackagesDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.DoctorFeature;
using Clinic_Booking.Entities.DoctorSubscription;
using Clinic_Booking.Enums;
using Clinic_Booking.Extensions;
using Clinic_Booking.IServices.IDoctorSubscriptionServices;
using Clinic_Booking.IServices.ILoadServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.DoctorSubscriptionServices
{
    public class DoctorSubscriptionServices : IDoctorSubscriptionServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;
        public DoctorSubscriptionServices(ApplicationDbContext context,ILoadServices load)
        {
            _context = context;
            _load = load;
        }
        public async Task<ActionResult<PaginationDto.PageResult<GetDoctorSubscriptionDto>>> GetListAsync(SearchDoctorSubscriptionDto form,int page = 1, int pageSize = 10)
        {
            try
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
                var now = DateTime.UtcNow;

                var query = _context.DoctorSubscriptions
                    .Include(ds => ds.Doctor).ThenInclude(i=> i.Specialization)
                    .Include(ds => ds.Package)
                    .Where(d=>form.Id== null || d.Id == form.Id)
                    .Where(d=>form.DoctorId== null || d.DoctorId == form.DoctorId)
                    .Where(d=>form.PackageId== null || d.PackageId == form.PackageId)
                    .Where(d=>form.Status == null || d.Status == form.Status);
                if (form.IsActive.HasValue)
                {
                    if (form.IsActive == true)
                    {
                        query = query.Where(ds =>
                            ds.Status == SubscriptionStatus.Active &&
                            ds.StartDate <= now &&
                            ds.EndDate >= now);
                    }
                    else
                    {
                        query = query.Where(ds =>
                            ds.Status != SubscriptionStatus.Active ||
                            ds.StartDate > now ||
                            ds.EndDate < now);

                    }
                }
                


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

                var docs = await query
                    .OrderBy(d => d.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(ds => new GetDoctorSubscriptionDto
                    {
                        Id = ds.Id,
                        StartDate = ds.StartDate,
                        EndDate = ds.EndDate,
                        IsActive =
                            ds.Status == SubscriptionStatus.Active &&
                            ds.StartDate <= now &&
                            ds.EndDate >= now,
                        Status = ds.Status == SubscriptionStatus.Active && ds.EndDate < now
                            ? SubscriptionStatus.Expired
                            : ds.Status,
                        CancelledAt = ds.CancelledAt,
                        Doctor = new GetDoctorDto
                        {
                            Id = ds.Doctor.Id,
                            Name = ds.Doctor.Name,
                            NormalizedName = ds.Doctor.NormalizedName,
                            Specialization = new DTOs.SharedDTO.GetItemsDto
                            {
                                Id = ds.Doctor.SpecializationId,
                                Name = ds.Doctor.Specialization.Name,
                                NormalizedName = ds.Doctor.Specialization.NormalizedName,
                            },
                            Description = ds.Doctor.Description,
                            SubscriptionRank = ds.Doctor.SubscriptionRank,
                            IraqiProvince = ds.Doctor.IraqiProvince,
                            IraqiProvinceName = ds.Doctor.IraqiProvince.GetDisplayName(),
                            IraqiProvinceNormalizedName = ds.Doctor.IraqiProvince.ToString(),
                            BirthDay = ds.Doctor.BirthDay,
                            ImageName = ds.Doctor.ImageName,
                            PhoneNumber = ds.Doctor.PhoneNumber,
                            Location = ds.Doctor.Location,
                            IsPubliclyVisible = ds.Doctor.IsPubliclyVisible,
                        },
                        Package = new GetSubscriptionPackages
                        {
                            Id = ds.Package.Id,
                            Name = ds.Package.Name,
                            NormalizedName = ds.Package.NormalizedName,
                            Price = ds.Package.Price,
                            YearlyPrice = ds.Package.YearlyPrice,
                            MaxClinics = ds.Package.MaxClinics,
                            MaxWeeklyDays = ds.Package.MaxWeeklyDays,
                            MaxDailyAppointments = ds.Package.MaxDailyAppointments,
                            ShowReviews = ds.Package.ShowReviews,
                            ShowMessages = ds.Package.ShowMessages,
                            MakeOffers = ds.Package.MakeOffers,
                            MaxActiveOffers = ds.Package.MaxActiveOffers,
                            EBooking = ds.Package.EBooking,
                            EPayments = ds.Package.EPayments
                        }
                    })
        .ToListAsync();

                var result = new PaginationDto.PageResult<GetDoctorSubscriptionDto>(docs, totalItems, totalPages, page, pageSize);

                return new OkObjectResult(new ResponseDto<PaginationDto.PageResult<GetDoctorSubscriptionDto>>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "تم جلب البيانات بنجاح!",
                    Data = result
                });
            }
            catch (DbUpdateException ex)
            {
                return new ObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "خطأ في قاعدة البيانات!",
                    Data = ex.InnerException?.Message ?? ex.Message
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
                });
            }
        }

        public async Task<IActionResult> CreateSubscriptionAsync(DoctorSubscriptionAddDto form)
        {
            var doctor = await _context.Doctors
                .Where(d => d.Id == form.DoctorId && !d.IsDeleted)
                .FirstOrDefaultAsync();

            if (doctor == null)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "يرجى اختيار عيادة اخرى!",
                    Data = null
                });
            }

            if (form.Status is not SubscriptionStatus.Pending and not SubscriptionStatus.Active)
            {
                return BadRequest("حالة الاشتراك الجديد يجب أن تكون قيد الانتظار أو نشطة.");
            }

            var now = DateTime.UtcNow;

            bool hasActive = await _context.DoctorSubscriptions
                .AnyAsync(ds =>
                    ds.DoctorId == form.DoctorId &&
                    ds.Status == SubscriptionStatus.Active &&
                    ds.StartDate <= now &&
                    ds.EndDate >= now);

            if (hasActive && form.Status == SubscriptionStatus.Active)
            {
                return BadRequest("الدكتور لديه اشتراك نشط بالفعل.");
            }

            var package = await _context.SubscriptionPackages
                .FirstOrDefaultAsync(p => p.Id == form.PackageId);

            if (package == null)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "الباقة غير موجودة!",
                    Data = null
                });
            }

            var subscription = new DoctorSubscription
            {
                DoctorId = form.DoctorId,
                PackageId = form.PackageId,
                StartDate = now,
                EndDate = AddSubscriptionPeriod(now, form.IsYearly),
                Status = form.Status,
                CreatorId = _load.GetCurrentUserId(),
            };

            _context.DoctorSubscriptions.Add(subscription);
            if (form.Status == SubscriptionStatus.Active)
            {
                await SyncDoctorFeaturesAsync(form.DoctorId, package);
                doctor.SubscriptionRank++;
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = form.Status == SubscriptionStatus.Active
                    ? "تم إنشاء وتفعيل الاشتراك بنجاح."
                    : "تم إنشاء الاشتراك بحالة انتظار التفعيل.",
                Data = new { subscription.Id, subscription.Status, subscription.EndDate }
            });
        }

        public async Task<IActionResult> RemoveSubscriptionAsync(int id)
        {
            var subscription = await _context.DoctorSubscriptions
                .FirstOrDefaultAsync(ds => ds.Id == id && !ds.IsDeleted);

            if (subscription == null)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "لا يوجد اشتراك نشط !",
                    Data = null
                });
            }

            if (subscription.Status == SubscriptionStatus.Cancelled)
            {
                return BadRequest("الاشتراك ملغي مسبقاً.");
            }

            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.CancelledAt = DateTime.UtcNow;
            subscription.ModifierId = _load.GetCurrentUserId();
            subscription.ModifiedAt = DateTime.UtcNow;
            await DisableDoctorFeaturesIfNoActiveSubscriptionAsync(subscription.DoctorId, subscription.Id);

            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "تم إلغاء الاشتراك وتعطيل الميزات بنجاح.",
                Data = null
            });
        }

        public async Task<IActionResult> ActivateSubscriptionAsync(int id)
        {
            var subscription = await _context.DoctorSubscriptions
                .Include(ds => ds.Package)
                .FirstOrDefaultAsync(ds => ds.Id == id && !ds.IsDeleted);
            if (subscription == null)
            {
                return NotFound();
            }

            if (subscription.Status != SubscriptionStatus.Pending)
            {
                return BadRequest("يمكن تفعيل الاشتراكات قيد الانتظار فقط.");
            }

            var now = DateTime.UtcNow;
            var hasActive = await HasActiveSubscriptionAsync(subscription.DoctorId, now, subscription.Id);
            if (hasActive)
            {
                return BadRequest("يوجد اشتراك نشط للطبيب بالفعل.");
            }

            var subscriptionDuration = subscription.EndDate - subscription.StartDate;
            subscription.Status = SubscriptionStatus.Active;
            subscription.StartDate = now;
            subscription.EndDate = now.Add(subscriptionDuration);
            subscription.ModifierId = _load.GetCurrentUserId();
            subscription.ModifiedAt = now;
            await SyncDoctorFeaturesAsync(subscription.DoctorId, subscription.Package);
            await IncrementSubscriptionRankAsync(subscription.DoctorId);
            await _context.SaveChangesAsync();

            return Ok("تم تفعيل الاشتراك بنجاح.", new { subscription.Id, subscription.EndDate });
        }

        public async Task<IActionResult> RenewSubscriptionAsync(int id, RenewDoctorSubscriptionDto form)
        {
            var subscription = await _context.DoctorSubscriptions
                .Include(ds => ds.Package)
                .FirstOrDefaultAsync(ds => ds.Id == id && !ds.IsDeleted);
            if (subscription == null)
            {
                return NotFound();
            }

            if (subscription.Status == SubscriptionStatus.Cancelled)
            {
                return BadRequest("لا يمكن تجديد اشتراك ملغي. أنشئ اشتراكاً جديداً.");
            }

            var now = DateTime.UtcNow;
            if (await HasActiveSubscriptionAsync(subscription.DoctorId, now, subscription.Id))
            {
                return BadRequest("يوجد اشتراك نشط آخر للطبيب بالفعل.");
            }

            var periodStart = subscription.EndDate > now ? subscription.EndDate : now;
            subscription.StartDate = subscription.Status == SubscriptionStatus.Expired ? now : subscription.StartDate;
            subscription.EndDate = AddSubscriptionPeriod(periodStart, form.IsYearly);
            subscription.Status = SubscriptionStatus.Active;
            subscription.CancelledAt = null;
            subscription.ModifierId = _load.GetCurrentUserId();
            subscription.ModifiedAt = now;
            await SyncDoctorFeaturesAsync(subscription.DoctorId, subscription.Package);
            await IncrementSubscriptionRankAsync(subscription.DoctorId);
            await _context.SaveChangesAsync();

            return Ok("تم تجديد الاشتراك بنجاح.", new { subscription.Id, subscription.EndDate });
        }

        public async Task<IActionResult> UpgradeSubscriptionAsync(int id, UpgradeDoctorSubscriptionDto form)
        {
            var subscription = await _context.DoctorSubscriptions
                .Include(ds => ds.Package)
                .FirstOrDefaultAsync(ds => ds.Id == id && !ds.IsDeleted);
            if (subscription == null)
            {
                return NotFound();
            }

            if (subscription.Status != SubscriptionStatus.Active ||
                subscription.EndDate < DateTime.UtcNow)
            {
                return BadRequest("يجب أن يكون الاشتراك نشطاً قبل ترقيته.");
            }

            var package = await _context.SubscriptionPackages
                .FirstOrDefaultAsync(p => p.Id == form.PackageId && !p.IsDeleted);
            if (package == null)
            {
                return BadRequest("الباقة غير موجودة.");
            }

            if (package.Id == subscription.PackageId)
            {
                return BadRequest("الاشتراك يستخدم هذه الباقة بالفعل.");
            }

            if (package.Price <= subscription.Package.Price &&
                package.YearlyPrice <= subscription.Package.YearlyPrice)
            {
                return BadRequest("الباقة المحددة ليست ترقية للباقة الحالية.");
            }

            subscription.PackageId = package.Id;
            subscription.ModifierId = _load.GetCurrentUserId();
            subscription.ModifiedAt = DateTime.UtcNow;
            await SyncDoctorFeaturesAsync(subscription.DoctorId, package);
            await _context.SaveChangesAsync();

            return Ok("تمت ترقية الاشتراك بنجاح.", new { subscription.Id, subscription.PackageId });
        }

        private async Task SyncDoctorFeaturesAsync(int doctorId, Entities.SubscriptionPackage.SubscriptionPackage package)
        {
            var features = await _context.Features.Where(feature => !feature.IsDeleted).ToListAsync();
            var doctorFeatures = await _context.DoctorFeature
                .Where(feature => feature.DoctorId == doctorId && !feature.IsDeleted)
                .ToListAsync();

            foreach (var feature in features)
            {
                var doctorFeature = doctorFeatures.FirstOrDefault(item => item.FeatureId == feature.Id);
                if (doctorFeature == null)
                {
                    doctorFeature = new DoctorFeature
                    {
                        DoctorId = doctorId,
                        FeatureId = feature.Id,
                        CreatorId = _load.GetCurrentUserId()
                    };
                    _context.DoctorFeature.Add(doctorFeature);
                }

                doctorFeature.IsEnabled = IsFeatureAllowed(package, feature.NormalizedName);
                doctorFeature.ModifierId = _load.GetCurrentUserId();
                doctorFeature.ModifiedAt = DateTime.UtcNow;
            }
        }

        private async Task DisableDoctorFeaturesAsync(int doctorId)
        {
            var doctorFeatures = await _context.DoctorFeature
                .Where(feature => feature.DoctorId == doctorId && !feature.IsDeleted)
                .ToListAsync();
            foreach (var feature in doctorFeatures)
            {
                feature.IsEnabled = false;
                feature.ModifierId = _load.GetCurrentUserId();
                feature.ModifiedAt = DateTime.UtcNow;
            }
        }

        private async Task DisableDoctorFeaturesIfNoActiveSubscriptionAsync(int doctorId, int excludedId)
        {
            if (!await HasActiveSubscriptionAsync(doctorId, DateTime.UtcNow, excludedId))
            {
                await DisableDoctorFeaturesAsync(doctorId);
            }
        }

        private async Task<bool> HasActiveSubscriptionAsync(int doctorId, DateTime now, int? excludedId = null)
        {
            return await _context.DoctorSubscriptions.AnyAsync(subscription =>
                subscription.DoctorId == doctorId &&
                subscription.Id != excludedId &&
                subscription.Status == SubscriptionStatus.Active &&
                subscription.StartDate <= now &&
                subscription.EndDate >= now);
        }

        private async Task IncrementSubscriptionRankAsync(int doctorId)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId && !d.IsDeleted);
            if (doctor != null)
            {
                doctor.SubscriptionRank++;
            }
        }

        private static DateTime AddSubscriptionPeriod(DateTime startDate, bool isYearly)
        {
            return isYearly ? startDate.AddYears(1) : startDate.AddMonths(1);
        }

        private static bool IsFeatureAllowed(
            Entities.SubscriptionPackage.SubscriptionPackage package,
            string normalizedName)
        {
            return normalizedName switch
            {
                "ShowReviews" => package.ShowReviews,
                "ShowMessages" => package.ShowMessages,
                "MakeOffers" => package.MakeOffers,
                "EBooking" => package.EBooking,
                "EPayments" => package.EPayments,
                _ => false
            };
        }

        private static IActionResult Ok(string message, object data)
        {
            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = message,
                Data = data
            });
        }

        private static IActionResult BadRequest(string message)
        {
            return new BadRequestObjectResult(new ResponseDto<object>
            {
                Status = "Error",
                Code = 400,
                Message = message,
                Data = null
            });
        }

        private static IActionResult NotFound()
        {
            return new NotFoundObjectResult(new ResponseDto<object>
            {
                Status = "Error",
                Code = 404,
                Message = "الاشتراك غير موجود.",
                Data = null
            });
        }

    }
}
