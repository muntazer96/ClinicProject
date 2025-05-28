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
        [HttpDelete("DeleteAsync/{id}")]
        public async Task<IActionResult> SoftDeleteUserAsync(string id)
        {
            return await _service.SoftDeleteUserAsync(id);
        }
        [HttpPost("LockAsync/{id}")]
        public async Task<IActionResult> ToggleUserLockStatusAsync(string id)
        {
            return await _service.ToggleUserLockStatusAsync(id);
        }
        [HttpGet("GetListAsync")]
        public async Task<IActionResult> GetPaginatedUsersAsync(Guid UserGUID, int page = 1, int pageSize = 10)
        {
            return await _service.GetPaginatedUsersAsync(UserGUID, page, pageSize);
        }
        [HttpPost("UpProfileImage")]
        public async Task<IActionResult> UploadImgAsync(IFormFile file)
        {
            return await _service.UploadImgAsync(file);
        }
        [HttpPost("EmailConfirmation")]
        public async Task<IActionResult> SendEmailConfirmationAsync(string guid)
        {
            return await _service.SendEmailConfirmationAsync(guid);
        }
        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmailAsync(Guid userId, string token)
        {
            return await _service.ConfirmEmailAsync(userId, token);
        }
    }
}
