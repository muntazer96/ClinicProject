using Clinic_Booking.DTOs.DoctorDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.IDoctorServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{
    public class DoctorController : BaseApiController
    {
        private readonly IDoctorServices _services;

        public DoctorController(IDoctorServices services)
        {
            _services = services;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationDto.PageResult<GetDoctorDto>>> GetDoctorsAsync(
            [FromQuery] SearchDoctorDto filter,
            int page = 1,
            int pageSize = 10)
        {
            return await _services.GetListAsync(filter, page, pageSize);
        }

        [HttpGet("public")]
        public async Task<ActionResult<PaginationDto.PageResult<PublicDoctorListDto>>> SearchPublicAsync(
            [FromQuery] SearchDoctorDto filter,
            int page = 1,
            int pageSize = 10)
        {
            return await _services.SearchPublicAsync(filter, page, pageSize);
        }

        [HttpGet("public/{doctorId}")]
        public async Task<IActionResult> GetPublicProfileAsync(int doctorId)
        {
            return await _services.GetPublicProfileAsync(doctorId);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDoctorAsync(DoctorAddDto form)
        {
            return await _services.AddDoctorAsync(form);
        }

        [HttpGet("my")]
        [Authorize(Roles = "DoctorUser")]
        public async Task<IActionResult> GetMyProfileAsync()
        {
            return await _services.GetMyProfileAsync();
        }

        [HttpPut("my")]
        [Authorize(Roles = "DoctorUser")]
        public async Task<IActionResult> UpdateMyProfileAsync(DoctorProfileUpdateDto form)
        {
            return await _services.UpdateMyProfileAsync(form);
        }

        [HttpPut]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateDoctorAsync(DoctorUpdateDto form)
        {
            return await _services.UpdateDoctorAsync(form);
        }

        [HttpPost("{doctorId}/link-account")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> LinkAccountAsync(int doctorId, LinkDoctorAccountDto form)
        {
            return await _services.LinkAccountAsync(doctorId, form);
        }

        [HttpDelete("{doctorId}/link-account")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UnlinkAccountAsync(int doctorId)
        {
            return await _services.UnlinkAccountAsync(doctorId);
        }

        [HttpPut("{doctorId}/visibility")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateVisibilityAsync(int doctorId, DoctorVisibilityUpdateDto form)
        {
            return await _services.UpdateVisibilityAsync(doctorId, form);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctorAsync(int id)
        {
            return await _services.DeleteAsync(id);
        }
    }
}
