using Clinic_Booking.DTOs.DoctorFeatureDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.IDoctorFeatureServices;
using Clinic_Booking.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public class DoctorFeatureController : BaseApiController
    {
        private readonly IDoctorFeatureServices _services;

        public DoctorFeatureController(IDoctorFeatureServices services)
        {
            _services = services;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationDto.PageResult<GetDoctorFeatureDto>>> GetDoctorFeaturesAsync(
            [FromQuery] SearchDoctorFeatureDto filter,
            int page = 1,
            int pageSize = 10)
        {
            return await _services.GetListAsync(filter, page, pageSize);
        }

        [HttpPost("{id}/toggle")]
        public async Task<IActionResult> ToggleDoctorFeatureAsync(int id)
        {
            return await _services.ToggleDoctorFeatureAsync(id);
        }
    }
}
