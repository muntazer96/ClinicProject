using Clinic_Booking.Data;
using Clinic_Booking.DTOs.DoctorDTO;
using Clinic_Booking.DTOs.DoctorSubscriptionDTO;
using Clinic_Booking.DTOs.SubscriptionPackagesDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.DoctorFeature;
using Clinic_Booking.Entities.DoctorSubscription;
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
                    .Where(d=>form.PackageId== null || d.PackageId == form.PackageId);
                if (form.IsActive.HasValue)
                {
                    if (form.IsActive == true)
                    {
                        query = query.Where(ds => ds.StartDate <= now && ds.EndDate >= now);
                    }
                    else
                    {
                        query = query.Where(ds => ds.StartDate >= now && ds.EndDate <= now);

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
                        IsActive = ds.StartDate <= now && ds.EndDate >= now,
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
                        },
                        Package = new GetSubscriptionPackages
                        {
                            Id = ds.Package.Id,
                            Name = ds.Package.Name,
                            NormalizedName = ds.Package.NormalizedName,
                            Price = ds.Package.Price,
                            YearlyPrice = ds.Package.YearlyPrice,
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

            var now = DateTime.UtcNow;

            bool hasActive = await _context.DoctorSubscriptions
                .AnyAsync(ds => ds.DoctorId == form.DoctorId && ds.StartDate <= now && ds.EndDate >= now);

            if (hasActive)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "الدكتور لديه اشتراك نشط بالفعل!",
                    Data = null
                });
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
                EndDate = now.AddMonths(1),
                CreatorId = _load.GetCurrentUserId(),
            };

            _context.DoctorSubscriptions.Add(subscription);

            // 🔁 Add matching features to DoctorFeature table
            var features = await _context.Features.ToListAsync();
            var doctorFeatures = new List<DoctorFeature>();

            foreach (var feature in features)
            {
                bool isEnabled = feature.NormalizedName switch
                {
                    "ShowReviews" => package.ShowReviews,
                    "ShowMessages" => package.ShowMessages,
                    "MakeOffers" => package.MakeOffers,
                    "EBooking" => package.EBooking,
                    "EPayments" => package.EPayments,
                    _ => false
                };

                doctorFeatures.Add(new DoctorFeature
                {
                    DoctorId = form.DoctorId,
                    FeatureId = feature.Id,
                    IsEnabled = isEnabled,
                    CreatorId = _load.GetCurrentUserId()
                });
            }

            _context.DoctorFeature.AddRange(doctorFeatures);

            // Update doctor's subscription rank
            var currentRank = doctor.SubscriptionRank;
            doctor.SubscriptionRank = currentRank + 1;

            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "تم إنشاء الاشتراك بنجاح.",
                Data = null
            });
        }

        public async Task<IActionResult> RemoveSubscriptionAsync(int id)
        {
            var now = DateTime.UtcNow;

            // Get active subscription
            var subscription = await _context.DoctorSubscriptions
                .Where(ds => ds.Id == id && ds.StartDate <= now && ds.EndDate >= now)
                .OrderByDescending(ds => ds.StartDate)
                .FirstOrDefaultAsync();

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

            // Option 1: Hard delete the subscription
            //_context.DoctorSubscriptions.Remove(subscription);

            // Option 2: Mark subscription as expired by setting EndDate in the past
            subscription.EndDate = now.AddSeconds(-1);

            // Disable all features for this doctor
            var doctorFeatures = await _context.DoctorFeature
                .Where(df => df.DoctorId == subscription.DoctorId && !df.IsDeleted)
                .ToListAsync();

            foreach (var feature in doctorFeatures)
            {
                _context.DoctorFeature.Remove(feature);
            }

            // Optionally reduce subscription rank
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == subscription.DoctorId);
            if (doctor != null && doctor.SubscriptionRank > 0)
            {
                doctor.SubscriptionRank--;
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "تم إلغاء الاشتراك وتعطيل الميزات بنجاح.",
                Data = null
            });
        }

    }
}
