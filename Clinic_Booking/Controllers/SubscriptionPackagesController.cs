using Clinic_Booking.Authorization;
using Clinic_Booking.DTOs.SubscriptionPackagesDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.ISubscriptionPackagesServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Clinic_Booking.Controllers
{
    public class SubscriptionPackagesController : BaseApiController
    {
        private readonly ISubscriptionPackagesServices _service;

        public SubscriptionPackagesController(ISubscriptionPackagesServices services)
        {
            _service = services;
        }

        [HttpGet]
        [EnableRateLimiting("PublicRead")]
        public async Task<ActionResult<PaginationDto.PageResult<GetSubscriptionPackages>>> GetSubscriptionPackagesAsync(
            int page = 1, int pageSize = 10)
        {
            return await _service.GetListAsync(page, pageSize);
        }

        [HttpPut]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> UpdateSubscriptionPackageAsync(UpdateSubscriptionPackagesDto form)
        {
            return await _service.UpdateAsync(form);
        }
    }
}
