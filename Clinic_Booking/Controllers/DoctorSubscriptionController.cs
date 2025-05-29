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

        [HttpGet]
        public async Task<ActionResult<PaginationDto.PageResult<GetDoctorSubscriptionDto>>> GetDoctorSubscriptionsAsync(
            [FromQuery] SearchDoctorSubscriptionDto filter,
            int page = 1,
            int pageSize = 10)
        {
            return await _services.GetListAsync(filter, page, pageSize);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDoctorSubscriptionAsync([FromBody] DoctorSubscriptionAddDto form)
        {
            return await _services.CreateSubscriptionAsync(form);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctorSubscriptionAsync(int id)
        {
            return await _services.RemoveSubscriptionAsync(id);
        }
    }
}
