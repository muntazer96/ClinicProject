using Clinic_Booking.Data;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.IFeatureServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.FeatureServices
{
    public class FeatureServices : IFeatureServices
    {
        private readonly ApplicationDbContext _context;
        public FeatureServices(ApplicationDbContext context) 
        { 
            _context = context;
        }
        public async Task<IActionResult> GetItemsAsync()
        {
            try
            {
                var days = await _context.Features
                    .Where(d => !d.IsDeleted)
                    .Select(d => new
                    {
                        d.Id,
                        d.Name,
                        d.NormalizedName
                    })
                    .ToListAsync();

                if (days == null || days.Count == 0)
                {
                    return new NotFoundObjectResult(new ResponseDto<string>
                    {
                        Status = "Not Found",
                        Code = 404,
                        Message = "لا توجد مميزات متاحة!",
                        Data = null
                    });
                }

                return new OkObjectResult(new ResponseDto<object>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "تم جلب المميزات بنجاح!",
                    Data = days
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
                })
                {
                    StatusCode = 500
                };
            }
        }
    }
}
