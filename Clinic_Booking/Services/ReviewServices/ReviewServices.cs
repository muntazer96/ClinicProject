using Clinic_Booking.Data;
using Clinic_Booking.DTOs.ReviewDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.Review;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.IPushNotificationServices;
using Clinic_Booking.IServices.IReviewServices;
using Clinic_Booking.Services.NotificationDeliveryServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.ReviewServices
{
    public class ReviewServices : IReviewServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;
        private readonly IPushNotificationServices _pushNotificationServices;

        public ReviewServices(
            ApplicationDbContext context,
            ILoadServices load,
            IPushNotificationServices pushNotificationServices)
        {
            _context = context;
            _load = load;
            _pushNotificationServices = pushNotificationServices;
        }

        public async Task<IActionResult> GetByDoctorAsync(int doctorId)
        {
            if (doctorId <= 0 || !await _context.Doctors.AnyAsync(doctor =>
                doctor.Id == doctorId &&
                !doctor.IsDeleted &&
                doctor.IsPubliclyVisible))
            {
                return NotFound("Doctor not found or is not publicly visible.");
            }

            if (!await IsReviewFeatureEnabledAsync(doctorId))
            {
                return BadRequest("Reviews are not available for this doctor.");
            }

            return Ok(await GetDoctorReviewsAsync(doctorId, true), "Doctor reviews retrieved successfully.");
        }

        public async Task<IActionResult> GetMineForDoctorAsync()
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var doctorId = await _context.Doctors
                .Where(doctor => doctor.UserId == userId && !doctor.IsDeleted)
                .Select(doctor => (int?)doctor.Id)
                .FirstOrDefaultAsync();
            if (!doctorId.HasValue)
            {
                return NotFound("Linked doctor account not found.");
            }

            var isEnabled = await IsReviewFeatureEnabledAsync(doctorId.Value);
            return Ok(await GetDoctorReviewsAsync(doctorId.Value, isEnabled), "Doctor reviews retrieved successfully.");
        }

        public async Task<IActionResult> AddAsync(AddReviewDto form)
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return Unauthorized();
            }

            if (form.Rating is < 1 or > 5)
            {
                return BadRequest("Rating must be between 1 and 5.");
            }

            if (form.Comment?.Length > 1000)
            {
                return BadRequest("Comment cannot exceed 1000 characters.");
            }

            if (!await IsReviewFeatureEnabledAsync(form.DoctorId))
            {
                return BadRequest("Reviews are not enabled for this doctor.");
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(item =>
                    item.Id == form.AppointmentId &&
                    item.DoctorId == form.DoctorId &&
                    item.UserId == userId &&
                    item.Status == AppointmentStatus.Completed &&
                    !item.IsDeleted);

            if (appointment == null)
            {
                return BadRequest("You can only review a doctor after completing a real booking with that doctor.");
            }

            if (await _context.Reviews.AnyAsync(review =>
                review.AppoinmentId == form.AppointmentId &&
                !review.IsDeleted))
            {
                return BadRequest("This booking has already been reviewed.");
            }

            var review = new Review
            {
                DoctorId = form.DoctorId,
                UserId = userId.Value,
                Rating = form.Rating,
                Comment = form.Comment?.Trim() ?? string.Empty,
                AppoinmentId = appointment.Id,
                CreatorId = userId
            };

            _context.Reviews.Add(review);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("This booking has already been reviewed.");
            }

            await NotifyDoctorAboutReviewAsync(review);

            return Ok(new { ReviewId = review.Id }, "Review added successfully.");
        }

        private async Task NotifyDoctorAboutReviewAsync(Review review)
        {
            var doctorUserId = await _context.Doctors
                .Where(doctor => doctor.Id == review.DoctorId && !doctor.IsDeleted && doctor.UserId.HasValue)
                .Select(doctor => doctor.UserId)
                .FirstOrDefaultAsync();

            if (!doctorUserId.HasValue)
            {
                return;
            }

            const string title = "وصل تقييم جديد";
            var body = $"قام أحد المراجعين بتقييمك {review.Rating} من 5 بعد الحجز رقم {review.AppoinmentId}.";
            var data = new Dictionary<string, string>
            {
                ["type"] = "review",
                ["reviewId"] = review.Id.ToString(),
                ["doctorId"] = review.DoctorId.ToString(),
                ["appointmentId"] = review.AppoinmentId?.ToString() ?? string.Empty,
                ["rating"] = review.Rating.ToString()
            };

            var sent = await _pushNotificationServices.SendToUserAsync(
                doctorUserId.Value,
                title,
                body,
                data);
            NotificationDeliveryAttemptRecorder.AddPushAttempt(
                _context,
                sent,
                doctorUserId.Value,
                title,
                body,
                data,
                doctorId: review.DoctorId,
                appointmentId: review.AppoinmentId);
            await _context.SaveChangesAsync();
        }

        private Task<bool> IsReviewFeatureEnabledAsync(int doctorId)
        {
            var now = DateTime.UtcNow;
            return _context.Doctors.AnyAsync(doctor =>
                doctor.Id == doctorId &&
                !doctor.IsDeleted &&
                doctor.DoctorSubscriptions.Any(subscription =>
                    subscription.Status == SubscriptionStatus.Active &&
                    subscription.StartDate <= now &&
                    subscription.EndDate >= now &&
                    subscription.Package.ShowReviews) &&
                doctor.DoctorFeatures.Any(feature =>
                    !feature.IsDeleted &&
                    feature.IsEnabled &&
                    feature.Feature.NormalizedName == "ShowReviews"));
        }

        private async Task<GetDoctorReviewsDto> GetDoctorReviewsAsync(int doctorId, bool isEnabled)
        {
            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(review => review.DoctorId == doctorId && !review.IsDeleted)
                .OrderByDescending(review => review.CreatedAt)
                .Select(review => new GetReviewDto
                {
                    Id = review.Id,
                    User = new GetUserReview
                    {
                        Id = review.UserId,
                        Name = review.User.Name,
                        NormalizedName = review.User.NormalizedUserName
                    },
                    Doctor = new GetDoctorReview
                    {
                        Id = review.Doctor.Id,
                        Name = review.Doctor.Name,
                        NormalizedName = review.Doctor.NormalizedName
                    },
                    Rating = review.Rating,
                    Comment = review.Comment,
                    AppoinmentId = review.AppoinmentId,
                    Appointment = review.Appointment == null
                        ? null
                        : new GetAppointmentReview
                        {
                            Id = review.Appointment.Id,
                            Status = review.Appointment.Status
                        }
                })
                .ToListAsync();

            return new GetDoctorReviewsDto
            {
                DoctorId = doctorId,
                IsEnabled = isEnabled,
                AverageRating = reviews.Count == 0 ? null : reviews.Average(review => review.Rating),
                ReviewCount = reviews.Count,
                Reviews = reviews
            };
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

        private static IActionResult Conflict(string message)
        {
            return new ConflictObjectResult(new ResponseDto<string>
            {
                Status = "Error",
                Code = 409,
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
                Message = "You must sign in before adding a review."
            });
        }
    }
}
