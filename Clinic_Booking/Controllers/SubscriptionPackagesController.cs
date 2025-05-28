using Clinic_Booking.DTOs.SubscriptionPackagesDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.ISubscriptionPackagesServices;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{
    public class SubscriptionPackagesController : BaseApiController
    {
        private readonly ISubscriptionPackagesServices _service;
        public SubscriptionPackagesController(ISubscriptionPackagesServices services)
        {
            _service = services;
        }
        [HttpGet("GetList")]
        public async Task<ActionResult<PaginationDto.PageResult<GetSubscriptionPackages>>> GetListAsync(int page = 1, int pageSize = 10)
        {
            return await _service.GetListAsync(page, pageSize);
        }
    }
}
