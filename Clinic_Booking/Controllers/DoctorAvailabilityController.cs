using Clinic_Booking.DTOs.DoctorAvailabilityDTO;
using Clinic_Booking.IServices.IDoctorAvailabilityServices;
using Clinic_Booking.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Clinic_Booking.Controllers
{
    public class DoctorAvailabilityController : BaseApiController
    {
        private readonly IDoctorAvailabilityServices _services;

        public DoctorAvailabilityController(IDoctorAvailabilityServices services)
        {
            _services = services;
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> CreateOrUpdateWeeklyAvailabilityAsync(AddDoctorAvailabilityDto dto)
        {
            return await _services.UpsertWeeklyAvailabilityAsync(dto);
        }

        [HttpGet("{clinicId}")]
        [EnableRateLimiting("PublicRead")]
        public async Task<IActionResult> GetWeeklyAvailabilityByClinicIdAsync(int clinicId)
        {
            return await _services.GetWeeklyAvailabilityAsync(clinicId);
        }

        [HttpPut("single-day")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> UpdateSingleDayAvailabilityAsync(UpdateSingleDayAvailabilityDto dto)
        {
            return await _services.UpdateSingleDayAvailabilityAsync(dto);
        }
    }
}
