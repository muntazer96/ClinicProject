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

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMineAsync(int page = 1, int pageSize = 20)
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return Unauthorized("You must sign in to view notifications.");
            }

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var notifications = await _context.Notifications
                .Where(notification =>
                    notification.UserId == userId &&
                    notification.DoctorId == null &&
                    !notification.IsDeleted)
                .OrderByDescending(notification => notification.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(notification => new
                {
                    notification.Id,
                    notification.Message,
                    notification.CreatedAt,
                    notification.Status,
                    notification.ReadAt
                })
                .ToListAsync();

            return Ok(notifications, "Notifications retrieved successfully.");
        }

        [HttpGet("my/unread-count")]
        [Authorize]
        public async Task<IActionResult> GetMineUnreadCountAsync()
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return Unauthorized("You must sign in to view notifications.");
            }

            var count = await _context.Notifications.CountAsync(notification =>
                notification.UserId == userId &&
                notification.DoctorId == null &&
                notification.Status == NotificationStatus.Unread &&
                !notification.IsDeleted);

            return Ok(new { UnreadCount = count }, "Unread notifications count retrieved successfully.");
        }

        [HttpPost("my/{id}/read")]
        [Authorize]
        public async Task<IActionResult> MarkMineAsReadForUserAsync(int id)
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return Unauthorized("You must sign in to view notifications.");
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.UserId == userId &&
                    item.DoctorId == null &&
                    !item.IsDeleted);
            if (notification == null)
            {
                return NotFound("Notification not found.");
            }

            notification.Status = NotificationStatus.Read;
            notification.ReadAt ??= BusinessClock.Now();
            notification.ModifiedAt = BusinessClock.Now();
            notification.ModifierId = userId;
            await _context.SaveChangesAsync();

            return Ok<object>(null, "Notification marked as read.");
        }

        [HttpPost("my/read-all")]
        [Authorize]
        public async Task<IActionResult> MarkAllMineAsReadForUserAsync()
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return Unauthorized("You must sign in to view notifications.");
            }

            var now = BusinessClock.Now();
            var notifications = await _context.Notifications
                .Where(item =>
                    item.UserId == userId &&
                    item.DoctorId == null &&
                    item.Status == NotificationStatus.Unread &&
                    !item.IsDeleted)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.Status = NotificationStatus.Read;
                notification.ReadAt ??= now;
                notification.ModifiedAt = now;
                notification.ModifierId = userId;
            }

            if (notifications.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return Ok<object>(null, "All notifications marked as read.");
        }

        [HttpGet("doctor/my")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> GetMineForDoctorAsync(int page = 1, int pageSize = 20)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue)
            {
                return Unauthorized();
            }

            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 50);

            var notifications = await _context.Notifications
                .Where(notification =>
                    notification.DoctorId == doctorId &&
                    !notification.IsDeleted)
                .OrderByDescending(notification => notification.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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

        [HttpGet("doctor/my/unread-count")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> GetMineUnreadCountForDoctorAsync()
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue)
            {
                return Unauthorized();
            }

            var count = await _context.Notifications.CountAsync(notification =>
                notification.DoctorId == doctorId &&
                notification.Status == NotificationStatus.Unread &&
                !notification.IsDeleted);

            return Ok(new { UnreadCount = count }, "Doctor unread notifications count retrieved successfully.");
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
            notification.ReadAt ??= BusinessClock.Now();
            notification.ModifiedAt = BusinessClock.Now();
            notification.ModifierId = _load.GetCurrentUserId();
            await _context.SaveChangesAsync();

            return Ok<object>(null, "Notification marked as read.");
        }

        [HttpPost("doctor/my/read-all")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> MarkAllMineAsReadForDoctorAsync()
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue)
            {
                return Unauthorized();
            }

            var userId = _load.GetCurrentUserId();
            var now = BusinessClock.Now();
            var notifications = await _context.Notifications
                .Where(item =>
                    item.DoctorId == doctorId &&
                    item.Status == NotificationStatus.Unread &&
                    !item.IsDeleted)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.Status = NotificationStatus.Read;
                notification.ReadAt ??= now;
                notification.ModifiedAt = now;
                notification.ModifierId = userId;
            }

            if (notifications.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return Ok<object>(null, "All notifications marked as read.");
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

        private static IActionResult Unauthorized(string message = "You must sign in with a linked doctor account.")
        {
            return new UnauthorizedObjectResult(new ResponseDto<string>
            {
                Status = "Error",
                Code = 401,
                Message = message
            });
        }
    }
}
