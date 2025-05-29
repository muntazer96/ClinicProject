using Clinic_Booking.DTOs.DoctorDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.IDoctorServices;
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

        [HttpPost]
        public async Task<IActionResult> CreateDoctorAsync(DoctorAddDto form)
        {
            return await _services.AddDoctorAsync(form);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctorAsync(int id)
        {
            return await _services.DeleteAsync(id);
        }
    }
}
