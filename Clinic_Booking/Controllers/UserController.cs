using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.IUserServices;
using Clinic_Booking.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
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
        [EnableRateLimiting("Auth")]
        public async Task<IActionResult> SignInAsync([FromBody] SignInDto form)
        {
            return await _service.LoginAsync(form);
        }

        [HttpPost("refresh")]
        [EnableRateLimiting("Auth")]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenDto form)
        {
            return await _service.RefreshTokenAsync(form);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> RevokeRefreshTokenAsync([FromBody] RefreshTokenDto form)
        {
            return await _service.RevokeRefreshTokenAsync(form);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyProfileAsync()
        {
            return await _service.GetMyProfileAsync();
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMyProfileAsync([FromBody] UserUpdateDto form)
        {
            return await _service.UpdateMyProfileAsync(form);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> UpdateUserAsync(string id, [FromBody] UserUpdateDto form)
        {
            return await _service.UpdateUserAsync(id, form);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> SoftDeleteUserAsync(string id)
        {
            return await _service.SoftDeleteUserAsync(id);
        }

        [HttpPost("{id}/lock-toggle")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> ToggleUserLockStatusAsync(string id)
        {
            return await _service.ToggleUserLockStatusAsync(id);
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetUsersAsync(Guid userGuid, string? search, int page = 1, int pageSize = 10)
        {
            return await _service.GetPaginatedUsersAsync(userGuid, search, page, pageSize);
        }

        [HttpPost("profile-image")]
        [Authorize]
        public async Task<IActionResult> UploadProfileImageAsync(IFormFile file)
        {
            return await _service.UploadImgAsync(file);
        }

        [HttpPost("email-confirmation")]
        [EnableRateLimiting("AccountRecovery")]
        public async Task<IActionResult> SendEmailConfirmationAsync(string identifier)
        {
            return await _service.SendEmailConfirmationAsync(identifier);
        }

        [HttpGet("email-confirm")]
        public async Task<IActionResult> ConfirmEmailAsync(Guid userId, string token)
        {
            return await _service.ConfirmEmailAsync(userId, token);
        }

        [HttpPost("phone-confirmation")]
        [Authorize]
        [EnableRateLimiting("Otp")]
        public async Task<IActionResult> SendPhoneConfirmationAsync()
        {
            return await _service.SendPhoneConfirmationAsync();
        }

        [HttpPost("phone-confirm")]
        [Authorize]
        [EnableRateLimiting("Otp")]
        public async Task<IActionResult> ConfirmPhoneAsync([FromBody] ConfirmPhoneDto form)
        {
            return await _service.ConfirmPhoneAsync(form);
        }

        [HttpPost("password/reset-link")]
        [EnableRateLimiting("AccountRecovery")]
        public async Task<IActionResult> SendResetPasswordLinkAsync(string identifier)
        {
            return await _service.SendResetPasswordLinkAsync(identifier);
        }

        [HttpPost("password/reset")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto form)
        {
            return await _service.ResetPasswordAsync(form);
        }

        [HttpPost("password/change")]
        [Authorize]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordDto form)
        {
            return await _service.ChangePasswordAsync(form);
        }
    }
}
