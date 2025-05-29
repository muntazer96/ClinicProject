using Clinic_Booking.DTOs.DoctorSubscriptionDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.IDoctorSubscriptionServices;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{

    public class DoctorSubscriptionController : BaseApiController
    {
        private readonly IDoctorSubscriptionServices _services;
        public DoctorSubscriptionController(IDoctorSubscriptionServices services)
        {
            _services = services;
        }
        [HttpGet("GetAll")]
        public async Task<ActionResult<PaginationDto.PageResult<GetDoctorSubscriptionDto>>> GetListAsync([FromQuery]SearchDoctorSubscriptionDto form,int page = 1, int pageSize = 10)
        {
            return await _services.GetListAsync(form,page, pageSize);
        }
        [HttpPost("Subscrip")]
        public async Task<IActionResult> CreateSubscriptionAsync([FromBody]DoctorSubscriptionAddDto form)
        {
            return await _services.CreateSubscriptionAsync(form);
        }
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> RemoveSubscriptionAsync(int id)
        {
            return await _services.RemoveSubscriptionAsync(id);
        }

    }
}
