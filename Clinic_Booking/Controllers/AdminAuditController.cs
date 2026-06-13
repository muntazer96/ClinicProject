using Clinic_Booking.Authorization;
using Clinic_Booking.Data;
using Clinic_Booking.DTOs.AdminAuditDTO;
using Clinic_Booking.DTOs.UserDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Controllers
{
    public class AdminAuditController : BaseApiController
    {
        private readonly ApplicationDbContext _context;

        public AdminAuditController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("audit-logs")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetAuditLogsAsync(
            [FromQuery] AuditLogQueryDto filter,
            int page = 1,
            int pageSize = 20)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.AuditLogs
                .Where(log => !log.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Action))
            {
                query = query.Where(log => log.Action == filter.Action);
            }

            if (!string.IsNullOrWhiteSpace(filter.EntityType))
            {
                query = query.Where(log => log.EntityType == filter.EntityType);
            }

            if (filter.UserId.HasValue)
            {
                query = query.Where(log => log.UserId == filter.UserId);
            }

            if (filter.DoctorId.HasValue)
            {
                query = query.Where(log => log.DoctorId == filter.DoctorId);
            }

            if (filter.ClinicId.HasValue)
            {
                query = query.Where(log => log.ClinicId == filter.ClinicId);
            }

            if (filter.AppointmentId.HasValue)
            {
                query = query.Where(log => log.AppointmentId == filter.AppointmentId);
            }

            if (filter.SubscriptionId.HasValue)
            {
                query = query.Where(log => log.SubscriptionId == filter.SubscriptionId);
            }

            if (filter.From.HasValue)
            {
                query = query.Where(log => log.OccurredAt >= filter.From);
            }

            if (filter.To.HasValue)
            {
                query = query.Where(log => log.OccurredAt <= filter.To);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var items = await query
                .OrderByDescending(log => log.OccurredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(log => new AuditLogDto
                {
                    Id = log.Id,
                    Action = log.Action,
                    EntityType = log.EntityType,
                    EntityId = log.EntityId,
                    UserId = log.UserId,
                    DoctorId = log.DoctorId,
                    ClinicId = log.ClinicId,
                    AppointmentId = log.AppointmentId,
                    SubscriptionId = log.SubscriptionId,
                    Details = log.Details,
                    OccurredAt = log.OccurredAt
                })
                .ToListAsync();

            return Ok(
                new PaginationDto.PageResult<AuditLogDto>(items, totalItems, totalPages, page, pageSize),
                "Audit logs retrieved successfully.");
        }

        [HttpGet("notification-deliveries")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetNotificationDeliveriesAsync(
            [FromQuery] NotificationDeliveryAttemptQueryDto filter,
            int page = 1,
            int pageSize = 20)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.NotificationDeliveryAttempts
                .Where(attempt => !attempt.IsDeleted)
                .AsQueryable();

            if (filter.FailedOnly && string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(attempt => attempt.Status == "Failed");
            }

            if (!string.IsNullOrWhiteSpace(filter.Channel))
            {
                query = query.Where(attempt => attempt.Channel == filter.Channel);
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(attempt => attempt.Status == filter.Status);
            }

            if (filter.RecipientUserId.HasValue)
            {
                query = query.Where(attempt => attempt.RecipientUserId == filter.RecipientUserId);
            }

            if (!string.IsNullOrWhiteSpace(filter.RecipientAddress))
            {
                query = query.Where(attempt => attempt.RecipientAddress == filter.RecipientAddress);
            }

            if (filter.DoctorId.HasValue)
            {
                query = query.Where(attempt => attempt.DoctorId == filter.DoctorId);
            }

            if (filter.ClinicId.HasValue)
            {
                query = query.Where(attempt => attempt.ClinicId == filter.ClinicId);
            }

            if (filter.AppointmentId.HasValue)
            {
                query = query.Where(attempt => attempt.AppointmentId == filter.AppointmentId);
            }

            if (filter.From.HasValue)
            {
                query = query.Where(attempt => attempt.CreatedAt >= filter.From);
            }

            if (filter.To.HasValue)
            {
                query = query.Where(attempt => attempt.CreatedAt <= filter.To);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var items = await query
                .OrderByDescending(attempt => attempt.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(attempt => new NotificationDeliveryAttemptDto
                {
                    Id = attempt.Id,
                    Channel = attempt.Channel,
                    Status = attempt.Status,
                    RecipientUserId = attempt.RecipientUserId,
                    RecipientAddress = attempt.RecipientAddress,
                    Title = attempt.Title,
                    Body = attempt.Body,
                    AttemptCount = attempt.AttemptCount,
                    LastAttemptAt = attempt.LastAttemptAt,
                    NextAttemptAt = attempt.NextAttemptAt,
                    LastError = attempt.LastError,
                    DoctorId = attempt.DoctorId,
                    ClinicId = attempt.ClinicId,
                    AppointmentId = attempt.AppointmentId,
                    CreatedAt = attempt.CreatedAt
                })
                .ToListAsync();

            return Ok(
                new PaginationDto.PageResult<NotificationDeliveryAttemptDto>(items, totalItems, totalPages, page, pageSize),
                "Notification delivery attempts retrieved successfully.");
        }

        private static IActionResult Ok<T>(T data, string message)
        {
            return new OkObjectResult(new DTOs.UserDTO.ResponseDto<T>
            {
                Status = "Success",
                Code = 200,
                Message = message,
                Data = data
            });
        }
    }
}
