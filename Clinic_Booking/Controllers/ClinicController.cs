using Clinic_Booking.DTOs.ClinicDTO;
using Clinic_Booking.IServices.IClinicServices;
using Clinic_Booking.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{
    public class ClinicController : BaseApiController
    {
        private readonly IClinicServices _services;

        public ClinicController(IClinicServices services)
        {
            _services = services;
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetByDoctorAsync(int doctorId)
        {
            return await _services.GetByDoctorAsync(doctorId);
        }

        [HttpGet("doctor/{doctorId}/admin")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetByDoctorForAdminAsync(int doctorId)
        {
            return await _services.GetByDoctorForAdminAsync(doctorId);
        }

        [HttpGet("my")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> GetMineAsync()
        {
            return await _services.GetMineAsync();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            return await _services.GetByIdAsync(id);
        }

        [HttpPost("my")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> AddMineAsync(ClinicAddDto form)
        {
            return await _services.AddMineAsync(form);
        }

        [HttpPut("my")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> UpdateMineAsync(ClinicUpdateDto form)
        {
            return await _services.UpdateMineAsync(form);
        }

        [HttpDelete("my/{id}")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> DeleteMineAsync(int id)
        {
            return await _services.DeleteMineAsync(id);
        }
    }
}
