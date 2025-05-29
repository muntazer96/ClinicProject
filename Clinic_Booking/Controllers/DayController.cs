using Clinic_Booking.IServices.IDayServices;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{

    public class DayController : BaseApiController
    {
        private readonly IDayServices _services;
        public DayController(IDayServices services)
        {
            _services = services;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllDaysAsync()
        {
            return await _services.GetItemsAsync();
        }

    }
}
