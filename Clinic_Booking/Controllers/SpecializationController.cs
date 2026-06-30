using Clinic_Booking.IServices.ISpecializationServices;
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

        [HttpGet]
        public async Task<IActionResult> GetSpecializationsAsync()
        {
            return await _services.GetItemsAsync();
        }
    }
}
