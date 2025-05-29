using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.IUserServices;
using Microsoft.AspNetCore.Http;
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

        [HttpPost("signup")]
        public async Task<IActionResult> SignUpAsync([FromBody] SignUpDto form)
        {
            return await _service.CreateUserAsync(form);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignInAsync([FromBody] SignInDto form)
        {
            return await _service.LoginAsync(form);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteUserAsync(string id)
        {
            return await _service.SoftDeleteUserAsync(id);
        }

        [HttpPost("{id}/lock-toggle")]
        public async Task<IActionResult> ToggleUserLockStatusAsync(string id)
        {
            return await _service.ToggleUserLockStatusAsync(id);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersAsync(Guid userGuid, int page = 1, int pageSize = 10)
        {
            return await _service.GetPaginatedUsersAsync(userGuid, page, pageSize);
        }

        [HttpPost("profile-image")]
        public async Task<IActionResult> UploadProfileImageAsync(IFormFile file)
        {
            return await _service.UploadImgAsync(file);
        }

        [HttpPost("email-confirmation")]
        public async Task<IActionResult> SendEmailConfirmationAsync(string guid)
        {
            return await _service.SendEmailConfirmationAsync(guid);
        }

        [HttpPost("email-confirm")]
        public async Task<IActionResult> ConfirmEmailAsync(Guid userId, string token)
        {
            return await _service.ConfirmEmailAsync(userId, token);
        }

        [HttpPost("password/reset-link")]
        public async Task<IActionResult> SendResetPasswordLinkAsync(string guid)
        {
            return await _service.SendResetPasswordLinkAsync(guid);
        }

        [HttpPost("password/reset")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto form)
        {
            return await _service.ResetPasswordAsync(form);
        }
    }
}
