using Clinic_Booking.DTOs.UserDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IUserServices
{
    public interface IUserServices
    {
        Task<IActionResult> CreateUserAsync(SignUpDto form);
        Task<IActionResult> UpdateUserAsync(string id, UserUpdateDto form);
        Task<IActionResult> LoginAsync(SignInDto form);
        Task<IActionResult> RefreshTokenAsync(RefreshTokenDto form);
        Task<IActionResult> RevokeRefreshTokenAsync(RefreshTokenDto form);
        Task<IActionResult> SoftDeleteUserAsync(string id);
        Task<IActionResult> ToggleUserLockStatusAsync(string id);
        Task<IActionResult> GetPaginatedUsersAsync(Guid userGuid, string? search, int page = 1, int pageSize = 10);
        Task<IActionResult> UploadImgAsync(IFormFile file);
        Task<IActionResult> SendEmailConfirmationAsync(string email);
        Task<IActionResult> ConfirmEmailAsync(Guid userId, string token);
        Task<IActionResult> SendResetPasswordLinkAsync(string guid);
        Task<IActionResult> ResetPasswordAsync(ResetPasswordDto form);
        Task<IActionResult> ChangePasswordAsync(ChangePasswordDto form);
    }
}
