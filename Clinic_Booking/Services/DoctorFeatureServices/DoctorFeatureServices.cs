using Clinic_Booking.Data;
using Clinic_Booking.DTOs.DoctorDTO;
using Clinic_Booking.DTOs.DoctorFeatureDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Extensions;
using Clinic_Booking.IServices.IDoctorFeatureServices;
using Clinic_Booking.IServices.ILoadServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.DoctorFeatureServices
{
    public class DoctorFeatureServices : IDoctorFeatureServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;
        public DoctorFeatureServices(ApplicationDbContext context , ILoadServices load)
        {
            _context = context;
            _load = load;
        }
        public async Task<ActionResult<PaginationDto.PageResult<GetDoctorFeatureDto>>> GetListAsync(SearchDoctorFeatureDto form, int page = 1, int pageSize = 10)
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

                var query = _context.DoctorFeature
                    .Include(ds => ds.Doctor).ThenInclude(i => i.Specialization)
                    .Include(ds => ds.Feature)
                    .Where(d=>!d.IsDeleted)
                    .Where(d=> form.Id == null || d.Id == form.Id)
                    .Where(d=> form.DoctorId == null || d.DoctorId == form.DoctorId)

                    ;




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
                    .Select(ds => new GetDoctorFeatureDto
                    {
                        Id = ds.Id,
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
                        Feature = new DTOs.FeatureDTO.GetFeatureDto
                        {
                            Id = ds.Feature.Id,
                            Name = ds.Feature.Name,
                            Description = ds.Feature.Description,
                            IsPremiumOnly = ds.Feature.IsPremiumOnly,   
                            NormalizedName  = ds.Feature.NormalizedName
                        },
                        IsEnabled = ds.IsEnabled
                    })
        .ToListAsync();

                var result = new PaginationDto.PageResult<GetDoctorFeatureDto>(docs, totalItems, totalPages, page, pageSize);

                return new OkObjectResult(new ResponseDto<PaginationDto.PageResult<GetDoctorFeatureDto>>
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

        public async Task<IActionResult> ToggleDoctorFeatureAsync(int id)
        {
            var doctorFeature = await _context.DoctorFeature
                .Include(df => df.Doctor)
                .Include(df => df.Feature)
                .FirstOrDefaultAsync(df => df.Id == id && !df.IsDeleted);

            if (doctorFeature == null)
            {
                return new NotFoundObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "الميزة غير موجودة!",
                    Data = null
                });
            }

            // Only check on enabling
            if (!doctorFeature.IsEnabled)
            {
                var now = DateTime.UtcNow;

                var activeSubscription = await _context.DoctorSubscriptions
                    .Include(ds => ds.Package)
                    .Where(ds => ds.DoctorId == doctorFeature.DoctorId &&
                                 ds.StartDate <= now && ds.EndDate >= now)
                    .OrderByDescending(ds => ds.StartDate)
                    .FirstOrDefaultAsync();

                if (activeSubscription == null)
                {
                    return new BadRequestObjectResult(new ResponseDto<object>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "لا يوجد اشتراك نشط للطبيب!",
                        Data = null
                    });
                }

                var package = activeSubscription.Package;
                var normalizedName = doctorFeature.Feature.NormalizedName;

                // Check if the feature is allowed in the current package
                bool isFeatureAvailable = normalizedName switch
                {
                    "ShowReviews" => package.ShowReviews,
                    "ShowMessages" => package.ShowMessages,
                    "MakeOffers" => package.MakeOffers,
                    "EBooking" => package.EBooking,
                    "EPayments" => package.EPayments,
                    _ => false
                };

                if (!isFeatureAvailable)
                {
                    return new BadRequestObjectResult(new ResponseDto<object>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "الميزة غير متاحة في الاشتراك الحالي!",
                        Data = null
                    });
                }
            }

            // Toggle the feature
            doctorFeature.IsEnabled = !doctorFeature.IsEnabled;
            doctorFeature.ModifiedAt = DateTime.UtcNow;
            doctorFeature.ModifierId = _load.GetCurrentUserId();
            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = doctorFeature.IsEnabled ? "تم تفعيل الميزة." : "تم تعطيل الميزة.",
                Data = null
            });
        }

    }
}
