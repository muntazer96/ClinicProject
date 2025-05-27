using Clinic_Booking.DTOs.UserDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IUserServices
{
    public interface IUserServices
    {
        Task<IActionResult> CreateUserAsync(SignUpDto form);
        Task<IActionResult> LoginAsync(SignInDto form);
        Task<IActionResult> SoftDeleteUserAsync(string id);
        Task<IActionResult> ToggleUserLockStatusAsync(string id);
        Task<IActionResult> GetPaginatedUsersAsync(Guid UserGUID, int page = 1, int pageSize = 10);
    }
}
