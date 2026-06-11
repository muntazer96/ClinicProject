using Clinic_Booking.DTOs.SharedDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Enums;
using Clinic_Booking.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{
    public class IraqiProvinceController : BaseApiController
    {
        [HttpGet]
        public IActionResult GetItems()
        {
            var items = Enum.GetValues<IraqiProvince>()
                .Select(province => new GetItemsDto
                {
                    Id = (int)province,
                    Name = province.GetDisplayName(),
                    NormalizedName = province.ToString(),
                    IconName = string.Empty
                })
                .ToList();

            return Ok(new ResponseDto<List<GetItemsDto>>
            {
                Status = "Success",
                Code = 200,
                Message = "تم جلب المحافظات بنجاح.",
                Data = items
            });
        }
    }
}
