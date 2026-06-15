using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Data;
using Clinic_Booking.IServices.IUserServices;
using Clinic_Booking.IServices.IPushNotificationServices;
using Clinic_Booking.Authorization;
using Clinic_Booking.Services.NotificationDeliveryServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Clinic_Booking.Controllers
{
    public class UserController : BaseApiController
    {
        private readonly IUserServices _service;
        private readonly IPushNotificationServices _pushNotificationServices;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserServices service,
            IPushNotificationServices pushNotificationServices,
            ApplicationDbContext context,
            ILogger<UserController> logger)
        {
            _service = service;
            _pushNotificationServices = pushNotificationServices;
            _context = context;
            _logger = logger;
        }

        [HttpPost("signup")]
        [EnableRateLimiting("Auth")]
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
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadProfileImageAsync(IFormFile file)
        {
            return await _service.UploadImgAsync(file);
        }

        [HttpPut("~/api/Profile/image")]
        [Authorize]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UpdateProfileImageAsync(IFormFile file)
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

        [HttpPost("device-token")]
        [Authorize]
        public async Task<IActionResult> RegisterDeviceTokenAsync([FromBody] DeviceTokenDto form)
        {
            return await _service.RegisterDeviceTokenAsync(form);
        }

        [HttpDelete("device-token")]
        [Authorize]
        public async Task<IActionResult> DeleteDeviceTokenAsync([FromBody] DeviceTokenDto form)
        {
            return await _service.DeleteDeviceTokenAsync(form);
        }

        [HttpPost("device-token/test")]
        [Authorize]
        public async Task<IActionResult> SendTestPushNotificationAsync()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 401,
                    Message = "يرجى تسجيل الدخول."
                });
            }

            _logger.LogInformation("Manual test push requested. UserId={UserId}", userId);
            const string title = "اختبار الإشعارات";
            const string body = "إذا وصل هذا الإشعار فربط Firebase يعمل بشكل صحيح.";
            var data = new Dictionary<string, string>
            {
                ["type"] = "test",
                ["createdAt"] = BusinessClock.Now().ToString("O")
            };
            var sent = await _pushNotificationServices.SendToUserAsync(
                userId,
                title,
                body,
                data);
            NotificationDeliveryAttemptRecorder.AddPushAttempt(_context, sent, userId, title, body, data);
            await _context.SaveChangesAsync();

            return Ok(new ResponseDto<string>
            {
                Status = "Success",
                Code = 200,
                Message = "تمت محاولة إرسال إشعار تجريبي. راقب لوكات الباك لمعرفة النتيجة."
            });
        }

        [HttpPost("{userId}/device-token/test")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> SendTestPushNotificationToUserAsync(Guid userId)
        {
            _logger.LogInformation("Manual admin test push requested. TargetUserId={UserId}", userId);
            const string title = "اختبار الإشعارات";
            const string body = "إذا وصل هذا الإشعار فربط Firebase يعمل بشكل صحيح.";
            var data = new Dictionary<string, string>
            {
                ["type"] = "test",
                ["createdAt"] = BusinessClock.Now().ToString("O")
            };
            var sent = await _pushNotificationServices.SendToUserAsync(
                userId,
                title,
                body,
                data);
            NotificationDeliveryAttemptRecorder.AddPushAttempt(_context, sent, userId, title, body, data);
            await _context.SaveChangesAsync();

            return Ok(new ResponseDto<string>
            {
                Status = "Success",
                Code = 200,
                Message = "تمت محاولة إرسال إشعار تجريبي للمستخدم المحدد. راقب لوكات الباك لمعرفة النتيجة."
            });
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
