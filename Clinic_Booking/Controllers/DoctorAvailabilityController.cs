using Clinic_Booking.DTOs.DoctorAvailabilityDTO;
using Clinic_Booking.IServices.IDoctorAvailabilityServices;
using Microsoft.AspNetCore.Http;
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
        [HttpPost("Create")]
        public async Task<IActionResult> SetWeeklyAvailabilityAsync(AddDoctorAvailabilityDto dto)
        {
            return await _services.SetWeeklyAvailabilityAsync(dto);
        }
    }
}
