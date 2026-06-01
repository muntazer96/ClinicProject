using Clinic_Booking.Data;
using Clinic_Booking.DTOs.ClinicExceptionDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.ClinicException;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.IClinicExceptionServices;
using Clinic_Booking.IServices.ILoadServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.ClinicExceptionServices
{
    public class ClinicExceptionServices : IClinicExceptionServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;

        public ClinicExceptionServices(ApplicationDbContext context, ILoadServices load)
        {
            _context = context;
            _load = load;
        }

        public async Task<IActionResult> GetMineAsync(int clinicId, DateOnly? fromDate, DateOnly? toDate)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue)
            {
                return Unauthorized();
            }

            if (!await OwnsClinicAsync(clinicId, doctorId.Value))
            {
                return NotFound("Clinic not found or you do not have permission to manage it.");
            }

            var query = _context.ClinicExceptions
                .Where(exception => exception.ClinicId == clinicId && !exception.IsDeleted);

            if (fromDate.HasValue)
            {
                var startDate = fromDate.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(exception => exception.ExceptionDate >= startDate);
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(exception => exception.ExceptionDate <= endDate);
            }

            var exceptionEntities = await query
                .OrderBy(exception => exception.ExceptionDate)
                .ToListAsync();

            var exceptions = exceptionEntities
                .Select(exception => new GetClinicExceptionDto
                {
                    Id = exception.Id,
                    ClinicId = exception.ClinicId,
                    ExceptionDate = DateOnly.FromDateTime(exception.ExceptionDate),
                    IsClosed = exception.IsClosed,
                    ClosureReason = exception.ClosureReason,
                    MaxAppointments = exception.MaxAppointments,
                    StartTime = exception.StartTime,
                    EndTime = exception.EndTime
                })
                .ToList();

            return Ok(exceptions, "Clinic exceptions retrieved successfully.");
        }

        public async Task<IActionResult> UpsertMineAsync(UpsertClinicExceptionDto form)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue)
            {
                return Unauthorized();
            }

            if (!await OwnsClinicAsync(form.ClinicId, doctorId.Value))
            {
                return NotFound("Clinic not found or you do not have permission to manage it.");
            }

            var validation = await ValidateAsync(form, doctorId.Value);
            if (validation != null)
            {
                return validation;
            }

            var exceptionDate = form.ExceptionDate.ToDateTime(TimeOnly.MinValue);
            var exception = form.Id.HasValue
                ? await _context.ClinicExceptions.FirstOrDefaultAsync(item =>
                    item.Id == form.Id &&
                    item.ClinicId == form.ClinicId &&
                    !item.IsDeleted)
                : await _context.ClinicExceptions.FirstOrDefaultAsync(item =>
                    item.ClinicId == form.ClinicId &&
                    item.ExceptionDate == exceptionDate &&
                    !item.IsDeleted);

            if (form.Id.HasValue && exception == null)
            {
                return NotFound("Clinic exception not found.");
            }

            if (exception == null)
            {
                exception = new ClinicException
                {
                    ClinicId = form.ClinicId,
                    ExceptionDate = exceptionDate,
                    CreatorId = _load.GetCurrentUserId()
                };
                _context.ClinicExceptions.Add(exception);
            }
            else
            {
                exception.ModifierId = _load.GetCurrentUserId();
                exception.ModifiedAt = DateTime.UtcNow;
            }

            exception.ExceptionDate = exceptionDate;
            exception.IsClosed = form.IsClosed;
            exception.ClosureReason = form.ClosureReason?.Trim();
            exception.MaxAppointments = form.MaxAppointments;
            exception.StartTime = form.StartTime;
            exception.EndTime = form.EndTime;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return BadRequest("Only one exception can be configured for a clinic on the same date.");
            }

            return Ok(new { exception.Id }, "Clinic exception saved successfully.");
        }

        public async Task<IActionResult> DeleteMineAsync(int id)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (!doctorId.HasValue)
            {
                return Unauthorized();
            }

            var exception = await _context.ClinicExceptions
                .Include(item => item.Clinic)
                .FirstOrDefaultAsync(item =>
                    item.Id == id &&
                    item.Clinic.DoctorId == doctorId &&
                    !item.IsDeleted);

            if (exception == null)
            {
                return NotFound("Clinic exception not found or you do not have permission to delete it.");
            }

            exception.IsDeleted = true;
            exception.DeleterId = _load.GetCurrentUserId();
            exception.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok<object>(null, "Clinic exception deleted successfully.");
        }

        private async Task<IActionResult?> ValidateAsync(UpsertClinicExceptionDto form, int doctorId)
        {
            if (form.ClinicId <= 0 || form.ExceptionDate == default)
            {
                return BadRequest("Clinic and exception date are required.");
            }

            if (form.ExceptionDate < DateOnly.FromDateTime(DateTime.Today))
            {
                return BadRequest("Clinic exceptions cannot be configured for a past date.");
            }

            if (form.ClosureReason?.Length > 500)
            {
                return BadRequest("Closure reason cannot exceed 500 characters.");
            }

            if (form.MaxAppointments is < 0)
            {
                return BadRequest("Maximum appointments cannot be negative.");
            }

            if (form.StartTime.HasValue != form.EndTime.HasValue ||
                form.StartTime.HasValue && form.StartTime >= form.EndTime)
            {
                return BadRequest("Display start and end times must be provided together, and the end time must be later.");
            }

            var now = DateTime.UtcNow;
            var packageLimit = await _context.DoctorSubscriptions
                .Where(subscription =>
                    subscription.DoctorId == doctorId &&
                    subscription.Status == SubscriptionStatus.Active &&
                    subscription.StartDate <= now &&
                    subscription.EndDate >= now)
                .OrderByDescending(subscription => subscription.StartDate)
                .Select(subscription => (int?)subscription.Package.MaxDailyAppointments)
                .FirstOrDefaultAsync();

            if (!packageLimit.HasValue)
            {
                return BadRequest("An active subscription is required to configure clinic exceptions.");
            }

            if (form.MaxAppointments > packageLimit.Value)
            {
                return BadRequest($"Maximum appointments cannot exceed your package limit ({packageLimit}).");
            }

            return null;
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

        private Task<bool> OwnsClinicAsync(int clinicId, int doctorId)
        {
            return _context.Clinics.AnyAsync(clinic =>
                clinic.Id == clinicId &&
                clinic.DoctorId == doctorId &&
                !clinic.IsDeleted);
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

        private static IActionResult BadRequest(string message)
        {
            return new BadRequestObjectResult(new ResponseDto<string>
            {
                Status = "Error",
                Code = 400,
                Message = message
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
