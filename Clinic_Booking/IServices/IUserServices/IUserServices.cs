using Clinic_Booking.DTOs.UserDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IUserServices
{
    public interface IUserServices
    {
        Task<IActionResult> CreateUserAsync(SignUpDto form);
        Task<IActionResult> LoginAsync(SignInDto form);
    }
}
