using Clinic_Booking.Authorization;
using Clinic_Booking.DTOs.DoctorOfferDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.IDoctorOfferServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Clinic_Booking.Controllers
{
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.DoctorUser}")]
    public class DoctorOfferController : BaseApiController
    {
        private readonly IDoctorOfferServices _services;

        public DoctorOfferController(IDoctorOfferServices services)
        {
            _services = services;
        }

        [HttpGet("public")]
        [AllowAnonymous]
        [EnableRateLimiting("PublicRead")]
        public async Task<ActionResult<PaginationDto.PageResult<DoctorOfferDto>>> GetPublicOffersAsync(
            [FromQuery] SearchDoctorOfferDto filter,
            int page = 1,
            int pageSize = 10)
        {
            return await _services.GetPublicAsync(filter, page, pageSize);
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<ActionResult<PaginationDto.PageResult<DoctorOfferDto>>> GetOffersAsync(
            [FromQuery] SearchDoctorOfferDto filter,
            int page = 1,
            int pageSize = 10)
        {
            return await _services.GetListAsync(filter, page, pageSize);
        }

        [HttpGet("my")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<ActionResult<PaginationDto.PageResult<DoctorOfferDto>>> GetMyOffersAsync(
            [FromQuery] SearchDoctorOfferDto filter,
            int page = 1,
            int pageSize = 10)
        {
            return await _services.GetMineAsync(filter, page, pageSize);
        }

        [HttpGet("quota/{doctorId}")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetQuotaAsync(int doctorId)
        {
            return await _services.GetQuotaAsync(doctorId);
        }

        [HttpGet("my/quota")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> GetMyQuotaAsync()
        {
            return await _services.GetMyQuotaAsync();
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> AddOfferAsync(DoctorOfferUpsertDto form)
        {
            return await _services.AddAsync(form);
        }

        [HttpPost("my")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> AddMyOfferAsync(DoctorOfferUpsertDto form)
        {
            return await _services.AddMineAsync(form);
        }

        [HttpPut]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> UpdateOfferAsync(DoctorOfferUpsertDto form)
        {
            return await _services.UpdateAsync(form);
        }

        [HttpPut("my")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> UpdateMyOfferAsync(DoctorOfferUpsertDto form)
        {
            return await _services.UpdateMineAsync(form);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> DeleteOfferAsync(int id)
        {
            return await _services.DeleteAsync(id);
        }

        [HttpDelete("my/{id}")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> DeleteMyOfferAsync(int id)
        {
            return await _services.DeleteMineAsync(id);
        }
    }
}
