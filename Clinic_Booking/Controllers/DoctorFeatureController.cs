using Clinic_Booking.DTOs.DoctorFeatureDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.IDoctorFeatureServices;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{

    public class DoctorFeatureController : BaseApiController
    {
        private readonly IDoctorFeatureServices _services;
        public DoctorFeatureController(IDoctorFeatureServices services)
        {
            _services = services;
        }
        [HttpGet("GetAll")]
        public async Task<ActionResult<PaginationDto.PageResult<GetDoctorFeatureDto>>> GetListAsync(int page = 1, int pageSize = 10)
        {
            return await _services.GetListAsync(page, pageSize);
        }
    }
}
