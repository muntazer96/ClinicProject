using Clinic_Booking.Authorization;
using Clinic_Booking.DTOs.AppReleaseDTO;
using Clinic_Booking.IServices.IAppReleaseServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{
    [Route("api/app-release")]
    public class AppReleaseController : BaseApiController
    {
        private readonly IAppReleaseServices _service;

        public AppReleaseController(IAppReleaseServices service)
        {
            _service = service;
        }

        [HttpPost("upload")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        [RequestSizeLimit(300 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 300 * 1024 * 1024)]
        public async Task<IActionResult> UploadAsync([FromForm] CreateAppReleaseDto dto)
        {
            return await _service.UploadAsync(dto);
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestAsync()
        {
            return await _service.GetLatestAsync();
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadAsync()
        {
            return await _service.DownloadAsync();
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetListAsync()
        {
            return await _service.GetListAsync();
        }

        [HttpPut("{id}/toggle-active")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> ToggleActiveAsync(int id)
        {
            return await _service.ToggleActiveAsync(id);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            return await _service.DeleteAsync(id);
        }
    }
}
