using Clinic_Booking.DTOs.DoctorAvailabilityDTO;
using Clinic_Booking.IServices.IDoctorAvailabilityServices;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> CreateOrUpdateWeeklyAvailabilityAsync(AddDoctorAvailabilityDto dto)
        {
            return await _services.UpsertWeeklyAvailabilityAsync(dto);
        }

        [HttpGet("{clinicId}")]
        public async Task<IActionResult> GetWeeklyAvailabilityByClinicIdAsync(int clinicId)
        {
            return await _services.GetWeeklyAvailabilityAsync(clinicId);
        }

        [HttpPut("single-day")]
        public async Task<IActionResult> UpdateSingleDayAvailabilityAsync(UpdateSingleDayAvailabilityDto dto)
        {
            return await _services.UpdateSingleDayAvailabilityAsync(dto);
        }
    }
}
