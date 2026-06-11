using Clinic_Booking.DTOs.DoctorSubscriptionDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.IDoctorSubscriptionServices;
using Clinic_Booking.Authorization;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("my/current")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> GetCurrentForDoctorAsync()
        {
            return await _services.GetCurrentForDoctorAsync();
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<ActionResult<PaginationDto.PageResult<GetDoctorSubscriptionDto>>> GetDoctorSubscriptionsAsync(
            [FromQuery] SearchDoctorSubscriptionDto filter,
            int page = 1,
            int pageSize = 10)
        {
            return await _services.GetListAsync(filter, page, pageSize);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> CreateDoctorSubscriptionAsync([FromBody] DoctorSubscriptionAddDto form)
        {
            return await _services.CreateSubscriptionAsync(form);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> DeleteDoctorSubscriptionAsync(int id)
        {
            return await _services.RemoveSubscriptionAsync(id);
        }

        [HttpPost("{id}/activate")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> ActivateDoctorSubscriptionAsync(int id)
        {
            return await _services.ActivateSubscriptionAsync(id);
        }

        [HttpPost("{id}/renew")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> RenewDoctorSubscriptionAsync(int id, RenewDoctorSubscriptionDto form)
        {
            return await _services.RenewSubscriptionAsync(id, form);
        }

        [HttpPost("{id}/upgrade")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> UpgradeDoctorSubscriptionAsync(int id, UpgradeDoctorSubscriptionDto form)
        {
            return await _services.UpgradeSubscriptionAsync(id, form);
        }
    }
}
