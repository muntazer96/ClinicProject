using Clinic_Booking.Authorization;
using Clinic_Booking.DTOs.AnalyticsDTO;
using Clinic_Booking.IServices.IAnalyticsServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{
    public class AnalyticsController : BaseApiController
    {
        private readonly IAnalyticsServices _analyticsServices;

        public AnalyticsController(IAnalyticsServices analyticsServices)
        {
            _analyticsServices = analyticsServices;
        }

        [HttpPost("track")]
        [AllowAnonymous]
        public async Task<IActionResult> TrackAsync([FromBody] TrackAnalyticsEventDto form)
        {
            return await _analyticsServices.TrackAsync(form);
        }

        [HttpGet("admin/summary")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetAdminSummaryAsync([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
        {
            return await _analyticsServices.GetAdminSummaryAsync(fromDate, toDate);
        }

        [HttpGet("doctor/summary")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> GetDoctorSummaryAsync([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
        {
            return await _analyticsServices.GetDoctorSummaryAsync(fromDate, toDate);
        }

        [HttpGet("admin/doctors/{doctorId:int}/summary")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetDoctorSummaryForAdminAsync(int doctorId, [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
        {
            return await _analyticsServices.GetDoctorSummaryForAdminAsync(doctorId, fromDate, toDate);
        }
    }
}
