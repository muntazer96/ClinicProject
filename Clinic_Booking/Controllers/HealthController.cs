using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "Healthy",
                serverTime = BusinessClock.Now(),
            });
        }
    }
}
