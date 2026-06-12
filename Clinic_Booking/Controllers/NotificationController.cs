using Clinic_Booking.Authorization;
using Clinic_Booking.Data;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.ILoadServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Controllers
{
    public class NotificationController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;

        public NotificationController(ApplicationDbContext context, ILoadServices load)
        {
            _context = context;
            _load = load;
        }

        [HttpGet("doctor/my")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> GetMineForDoctorAsync()
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue)
            {
                return Unauthorized();
            }

            var notifications = await _context.Notifications
                .Where(notification =>
                    notification.DoctorId == doctorId &&
                    !notification.IsDeleted)
                .OrderByDescending(notification => notification.CreatedAt)
                .Take(100)
                .Select(notification => new
                {
                    notification.Id,
                    notification.Message,
                    notification.CreatedAt,
                    notification.Status,
                    notification.ReadAt
                })
                .ToListAsync();

            return Ok(notifications, "Doctor notifications retrieved successfully.");
        }

        [HttpPost("doctor/my/{id}/read")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> MarkMineAsReadAsync(int id)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue)
            {
                return Unauthorized();
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.DoctorId == doctorId &&
                    !item.IsDeleted);
            if (notification == null)
            {
                return NotFound("Notification not found.");
            }

            notification.Status = NotificationStatus.Read;
            notification.ReadAt ??= DateTime.UtcNow;
            notification.ModifiedAt = DateTime.UtcNow;
            notification.ModifierId = _load.GetCurrentUserId();
            await _context.SaveChangesAsync();

            return Ok<object>(null, "Notification marked as read.");
        }

        private async Task<int?> GetCurrentDoctorIdAsync()
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return null;
            }

            return await _context.Doctors
                .Where(doctor => doctor.UserId == userId && !doctor.IsDeleted)
                .Select(doctor => (int?)doctor.Id)
                .FirstOrDefaultAsync();
        }

        private static IActionResult Ok<T>(T data, string message)
        {
            return new OkObjectResult(new ResponseDto<T>
            {
                Status = "Success",
                Code = 200,
                Message = message,
                Data = data
            });
        }

        private static IActionResult NotFound(string message)
        {
            return new NotFoundObjectResult(new ResponseDto<string>
            {
                Status = "Error",
                Code = 404,
                Message = message
            });
        }

        private static IActionResult Unauthorized()
        {
            return new UnauthorizedObjectResult(new ResponseDto<string>
            {
                Status = "Error",
                Code = 401,
                Message = "You must sign in with a linked doctor account."
            });
        }
    }
}
