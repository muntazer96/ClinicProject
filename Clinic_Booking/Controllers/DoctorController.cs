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
        [HttpGet("GetAll")]
        public async Task<ActionResult<PaginationDto.PageResult<GetDoctorDto>>> GetListAsync([FromQuery]SearchDoctorDto form,int page = 1, int pageSize = 10)
        {
            return await _services.GetListAsync(form,page, pageSize);
        }
        [HttpPost("CreateAsync")]
        public async Task<IActionResult> AddDoctorAsync(DoctorAddDto form)
        {
            return await _services.AddDoctorAsync(form);
        }
        [HttpDelete("DeleteAsync/{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            return await _services.DeleteAsync(id);
        }
    }
}
