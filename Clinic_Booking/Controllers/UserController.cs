using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.IUserServices;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{
    public class UserController : BaseApiController
    {
        private readonly IUserServices _service;
        public UserController(IUserServices service)
        {
            _service = service;
        }
        [HttpPost("SignUp")]
        public async Task<IActionResult> CreateUserAsync([FromBody] SignUpDto form)
        {
            return await _service.CreateUserAsync(form);
        }
        [HttpPost("SignIn")]
        public async Task<IActionResult> LoginAsync([FromBody] SignInDto form)
        {
            return await _service.LoginAsync(form);
        }
    }
}
