using Clinic_Booking.IServices.ISpecializationServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{

    public class SpecializationController : BaseApiController
    {
        private readonly ISpecializationServices _services;
        public SpecializationController(ISpecializationServices services)
        {
            _services = services;
        }
        [HttpGet("GetItems")]
        public async Task<IActionResult> GetItemsAsync()
        {
            return await _services.GetItemsAsync();
        }
    }
}
