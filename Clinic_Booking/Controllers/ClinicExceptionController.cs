using Clinic_Booking.DTOs.ClinicExceptionDTO;
using Clinic_Booking.IServices.IClinicExceptionServices;
using Clinic_Booking.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{
    public class ClinicExceptionController : BaseApiController
    {
        private readonly IClinicExceptionServices _services;

        public ClinicExceptionController(IClinicExceptionServices services)
        {
            _services = services;
        }

        [HttpGet("my/{clinicId}")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> GetMineAsync(
            int clinicId,
            [FromQuery] DateOnly? fromDate,
            [FromQuery] DateOnly? toDate)
        {
            return await _services.GetMineAsync(clinicId, fromDate, toDate);
        }

        [HttpPost("my")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> UpsertMineAsync(UpsertClinicExceptionDto form)
        {
            return await _services.UpsertMineAsync(form);
        }

        [HttpDelete("my/{id}")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> DeleteMineAsync(int id)
        {
            return await _services.DeleteMineAsync(id);
        }
    }
}
