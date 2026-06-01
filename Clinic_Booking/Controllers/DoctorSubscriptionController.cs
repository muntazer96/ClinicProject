using Clinic_Booking.DTOs.DoctorSubscriptionDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.IDoctorSubscriptionServices;
using Clinic_Booking.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{
    [Authorize(Roles = AppRoles.SuperAdmin)]
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

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateDoctorSubscriptionAsync(int id)
        {
            return await _services.ActivateSubscriptionAsync(id);
        }

        [HttpPost("{id}/renew")]
        public async Task<IActionResult> RenewDoctorSubscriptionAsync(int id, RenewDoctorSubscriptionDto form)
        {
            return await _services.RenewSubscriptionAsync(id, form);
        }

        [HttpPost("{id}/upgrade")]
        public async Task<IActionResult> UpgradeDoctorSubscriptionAsync(int id, UpgradeDoctorSubscriptionDto form)
        {
            return await _services.UpgradeSubscriptionAsync(id, form);
        }
    }
}
