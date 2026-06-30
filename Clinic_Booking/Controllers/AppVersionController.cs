using Clinic_Booking.Authorization;
using Clinic_Booking.DTOs.AppVersionDTO;
using Clinic_Booking.IServices.IAppVersionServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Clinic_Booking.Controllers
{
    public class AppVersionController : BaseApiController
    {
        private readonly IAppVersionServices _service;

        public AppVersionController(IAppVersionServices service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetListAsync()
        {
            return await _service.GetListAsync();
        }

        [HttpGet("check")]
        [EnableRateLimiting("PublicRead")]
        public async Task<IActionResult> CheckAsync(
            [FromQuery] string platform = "android",
            [FromQuery] string currentVersion = "1.0.0",
            [FromQuery] int currentBuildNumber = 1)
        {
            return await _service.CheckAsync(platform, currentVersion, currentBuildNumber);
        }

        [HttpPut]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> UpsertAsync(UpdateAppVersionPolicyDto dto)
        {
            return await _service.UpsertAsync(dto);
        }
    }
}
