using Clinic_Booking.Data;
using Clinic_Booking.DTOs.SubscriptionPackagesDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.ISubscriptionPackagesServices;
using Clinic_Booking.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.SubscriptionPackagesServices
{
    public class SubscriptionPackagesServices : ISubscriptionPackagesServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;
        public SubscriptionPackagesServices(ApplicationDbContext context, ILoadServices load)
        {
            _context = context;
            _load = load;
        }
        public async Task<ActionResult<PaginationDto.PageResult<GetSubscriptionPackages>>> GetListAsync(int page = 1, int pageSize = 10)
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

                var query = _context.SubscriptionPackages
                    .Where(d => !d.IsDeleted);

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

                var subs = await query
                    .OrderBy(d => d.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(d => new GetSubscriptionPackages
                    {
                        Id = d.Id,
                        Name = d.Name,
                        NormalizedName = d.NormalizedName,
                        Price = d.Price,
                        YearlyPrice = d.YearlyPrice,
                        MaxClinics = d.MaxClinics,
                        MaxWeeklyDays = d.MaxWeeklyDays,
                        MaxDailyAppointments = d.MaxDailyAppointments,
                        ShowReviews = d.ShowReviews,
                        ShowMessages = d.ShowMessages,
                        MakeOffers = d.MakeOffers,
                        MaxActiveOffers = d.MaxActiveOffers,
                        EBooking = d.EBooking,
                        EPayments = d.EPayments
                    })
                    .ToListAsync();

                var result = new PaginationDto.PageResult<GetSubscriptionPackages>(subs, totalItems, totalPages, page, pageSize);

                return new OkObjectResult(new ResponseDto<PaginationDto.PageResult<GetSubscriptionPackages>>
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
                    Message = "حدث خطأ أثناء حفظ البيانات في قاعدة البيانات.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "حدث خطأ غير متوقع، يرجى المحاولة لاحقاً.",
                    Data = null
                });
            }
        }

        public async Task<IActionResult> UpdateAsync(UpdateSubscriptionPackagesDto form)
        {
            try
            {
                var package = await _context.SubscriptionPackages
                    .FirstOrDefaultAsync(p => p.Id == form.Id && !p.IsDeleted);
                if (package == null)
                {
                    return new NotFoundObjectResult(new ResponseDto<string>
                    {
                        Status = "Error",
                        Code = 404,
                        Message = "الباقة غير موجودة.",
                        Data = null
                    });
                }

                package.Name = form.Name;
                package.NormalizedName = form.NormalizedName;
                package.Price = form.Price;
                package.YearlyPrice = form.YearlyPrice;
                package.MaxClinics = form.MaxClinics;
                package.MaxWeeklyDays = form.MaxWeeklyDays;
                package.MaxDailyAppointments = form.MaxDailyAppointments;
                package.ShowReviews = form.ShowReviews;
                package.ShowMessages = form.ShowMessages;
                package.EBooking = form.EBooking;
                package.EPayments = form.EPayments;
                package.MakeOffers = form.MakeOffers;
                package.MaxActiveOffers = form.MaxActiveOffers;
                package.ModifierId = _load.GetCurrentUserId();
                package.ModifiedAt = BusinessClock.Now();

                _context.SubscriptionPackages.Update(package);
                await _context.SaveChangesAsync();

                return new OkObjectResult(new ResponseDto<string>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "تم تحديث الباقة بنجاح!",
                    Data = null
                });
            }
            catch (DbUpdateException ex)
            {
                return new ObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "حدث خطأ أثناء حفظ البيانات في قاعدة البيانات.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "حدث خطأ غير متوقع، يرجى المحاولة لاحقاً.",
                    Data = null
                });
            }
        }
    }
}
