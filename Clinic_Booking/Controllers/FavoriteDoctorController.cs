using Clinic_Booking.Data;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.UserFavoriteDoctor;
using Clinic_Booking.Enums;
using Clinic_Booking.Extensions;
using Clinic_Booking.IServices.ILoadServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Controllers
{
    [Authorize]
    public class FavoriteDoctorController : BaseApiController
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;

        public FavoriteDoctorController(ApplicationDbContext context, ILoadServices load)
        {
            _context = context;
            _load = load;
        }

        [HttpGet]
        public async Task<IActionResult> GetMineAsync()
        {
            var userId = _load.GetCurrentUserId();
            if (!userId.HasValue || userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var now = DateTime.UtcNow;
            var doctors = await _context.UserFavoriteDoctors
                .Where(favorite =>
                    favorite.UserId == userId &&
                    !favorite.IsDeleted &&
                    !favorite.Doctor.IsDeleted &&
                    favorite.Doctor.IsPubliclyVisible)
                .OrderByDescending(favorite => favorite.CreatedAt)
                .Select(favorite => new
                {
                    favorite.Doctor.Id,
                    favorite.Doctor.Name,
                    favorite.Doctor.NormalizedName,
                    favorite.Doctor.SpecializationId,
                    SpecializationName = favorite.Doctor.Specialization.Name,
                    SpecializationNormalizedName = favorite.Doctor.Specialization.NormalizedName,
                    SpecializationIconName = favorite.Doctor.Specialization.IconName,
                    favorite.Doctor.Description,
                    favorite.Doctor.ImageName,
                    CanBookOnline =
                        favorite.Doctor.DoctorSubscriptions.Any(subscription =>
                            subscription.Status == SubscriptionStatus.Active &&
                            subscription.StartDate <= now &&
                            subscription.EndDate >= now &&
                            subscription.Package.EBooking) &&
                        favorite.Doctor.DoctorFeatures.Any(feature =>
                            !feature.IsDeleted &&
                            feature.IsEnabled &&
                            feature.Feature.NormalizedName == "EBooking"),
                    AverageRating = favorite.Doctor.Reviews
                        .Where(review => !review.IsDeleted)
                        .Select(review => (double?)review.Rating)
                        .Average(),
                    ReviewCount = favorite.Doctor.Reviews.Count(review => !review.IsDeleted),
                    IsFeatured = favorite.Doctor.DoctorSubscriptions.Any(subscription =>
                        subscription.Status == SubscriptionStatus.Active &&
                        subscription.StartDate <= now &&
                        subscription.EndDate >= now &&
                        subscription.Package.NormalizedName == "Premium"),
                    ActiveSubscriptionName = favorite.Doctor.DoctorSubscriptions
                        .Where(subscription =>
                            subscription.Status == SubscriptionStatus.Active &&
                            subscription.StartDate <= now &&
                            subscription.EndDate >= now)
                        .OrderByDescending(subscription => subscription.Package.YearlyPrice)
                        .ThenByDescending(subscription => subscription.Package.Price)
                        .Select(subscription => subscription.Package.Name)
                        .FirstOrDefault(),
                    ActiveSubscriptionNormalizedName = favorite.Doctor.DoctorSubscriptions
                        .Where(subscription =>
                            subscription.Status == SubscriptionStatus.Active &&
                            subscription.StartDate <= now &&
                            subscription.EndDate >= now)
                        .OrderByDescending(subscription => subscription.Package.YearlyPrice)
                        .ThenByDescending(subscription => subscription.Package.Price)
                        .Select(subscription => subscription.Package.NormalizedName)
                        .FirstOrDefault(),
                    ActiveSubscriptionWeight = favorite.Doctor.DoctorSubscriptions
                        .Where(subscription =>
                            subscription.Status == SubscriptionStatus.Active &&
                            subscription.StartDate <= now &&
                            subscription.EndDate >= now)
                        .Select(subscription => (decimal?)subscription.Package.YearlyPrice)
                        .Max() ?? 0,
                    Clinics = favorite.Doctor.Clinics
                        .Where(clinic => !clinic.IsDeleted && clinic.IsVisible)
                        .OrderBy(clinic => clinic.Id)
                        .Select(clinic => new
                        {
                            clinic.Id,
                            clinic.Name,
                            clinic.IraqiProvince,
                            IraqiProvinceName = clinic.IraqiProvince.GetDisplayName(),
                            clinic.Address,
                            ConsultationPrice = clinic.ShowConsultationPrice
                                ? clinic.ConsultationPrice
                                : null,
                            ShowConsultationPrice = clinic.ShowConsultationPrice &&
                                clinic.ConsultationPrice != null,
                            clinic.BookingWindowDays
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(doctors, "Favorite doctors retrieved successfully.");
        }

        [HttpGet("{doctorId}")]
        public async Task<IActionResult> IsFavoriteAsync(int doctorId)
        {
            var userId = _load.GetCurrentUserId();
            if (!userId.HasValue || userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var isFavorite = await _context.UserFavoriteDoctors.AnyAsync(favorite =>
                favorite.UserId == userId &&
                favorite.DoctorId == doctorId &&
                !favorite.IsDeleted);

            return Ok(new { isFavorite }, "Favorite state retrieved successfully.");
        }

        [HttpPost("{doctorId}")]
        public async Task<IActionResult> AddAsync(int doctorId)
        {
            var userId = _load.GetCurrentUserId();
            if (!userId.HasValue || userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var doctorExists = await _context.Doctors.AnyAsync(doctor =>
                doctor.Id == doctorId &&
                doctor.IsPubliclyVisible &&
                !doctor.IsDeleted);
            if (!doctorExists)
            {
                return NotFound("Doctor not found.");
            }

            var favorite = await _context.UserFavoriteDoctors.FirstOrDefaultAsync(item =>
                item.UserId == userId &&
                item.DoctorId == doctorId);
            if (favorite == null)
            {
                _context.UserFavoriteDoctors.Add(new UserFavoriteDoctor
                {
                    UserId = userId.Value,
                    DoctorId = doctorId,
                    CreatorId = userId
                });
            }
            else
            {
                favorite.IsDeleted = false;
                favorite.DeletedAt = null;
                favorite.DeleterId = null;
                favorite.ModifiedAt = DateTime.UtcNow;
                favorite.ModifierId = userId;
            }

            await _context.SaveChangesAsync();
            return Ok<object>(null, "Doctor added to favorites.");
        }

        [HttpDelete("{doctorId}")]
        public async Task<IActionResult> RemoveAsync(int doctorId)
        {
            var userId = _load.GetCurrentUserId();
            if (!userId.HasValue || userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var favorite = await _context.UserFavoriteDoctors.FirstOrDefaultAsync(item =>
                item.UserId == userId &&
                item.DoctorId == doctorId &&
                !item.IsDeleted);
            if (favorite == null)
            {
                return Ok<object>(null, "Doctor removed from favorites.");
            }

            favorite.IsDeleted = true;
            favorite.DeletedAt = DateTime.UtcNow;
            favorite.DeleterId = userId;
            await _context.SaveChangesAsync();

            return Ok<object>(null, "Doctor removed from favorites.");
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
    }
}
