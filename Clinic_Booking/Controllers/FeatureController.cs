using Clinic_Booking.IServices.IFeatureServices;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{

    public class FeatureController : BaseApiController
    {
        private readonly IFeatureServices _services;
        public FeatureController(IFeatureServices services)
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
