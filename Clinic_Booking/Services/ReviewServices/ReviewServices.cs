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
using Clinic_Booking.Services.ProfanityFilterService;

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

        public async Task<ActionResult<PaginationDto.PageResult<GetReviewDto>>> GetByDoctorAsync(int doctorId, int page = 1, int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return new BadRequestObjectResult(BadRequestDto("قيم الصفحة أو الحجم غير صحيحة."));

            if (doctorId <= 0 || !await _context.Doctors.AnyAsync(doctor =>
                doctor.Id == doctorId &&
                !doctor.IsDeleted &&
                doctor.IsPubliclyVisible))
            {
                return new NotFoundObjectResult(NotFoundDto("Doctor not found or is not publicly visible."));
            }

            if (!await IsReviewFeatureEnabledAsync(doctorId))
            {
                return new BadRequestObjectResult(BadRequestDto("Reviews are not available for this doctor."));
            }

            return new OkObjectResult(await GetReviewsPageAsync(doctorId, page, pageSize));
        }

        public async Task<IActionResult> GetMineForDoctorAsync(int page = 1, int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return new BadRequestObjectResult(BadRequestDto("قيم الصفحة أو الحجم غير صحيحة."));

            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return new UnauthorizedObjectResult(UnauthorizedDto());
            }

            var doctorId = await _context.Doctors
                .Where(doctor => doctor.UserId == userId && !doctor.IsDeleted)
                .Select(doctor => (int?)doctor.Id)
                .FirstOrDefaultAsync();
            if (!doctorId.HasValue)
            {
                return new NotFoundObjectResult(NotFoundDto("Linked doctor account not found."));
            }

            var isEnabled = await IsReviewFeatureEnabledAsync(doctorId.Value);

            var baseQuery = _context.Reviews
                .AsNoTracking()
                .Where(review => review.DoctorId == doctorId && !review.IsDeleted);

            var reviewCount = await baseQuery.CountAsync();
            var averageRating = reviewCount > 0
                ? await baseQuery.AverageAsync(review => (double)review.Rating)
                : (double?)null;

            var totalPages = (int)Math.Ceiling(reviewCount / (double)pageSize);

            var items = await baseQuery
                .OrderByDescending(review => review.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                    AppointmentId = review.AppointmentId,
                    Appointment = review.Appointment == null
                        ? null
                        : new GetAppointmentReview
                        {
                            Id = review.Appointment.Id,
                            Status = review.Appointment.Status
                        }
                })
                .ToListAsync();

            return new OkObjectResult(new ResponseDto<DoctorReviewsPageResultDto>
            {
                Status = "Success",
                Code = 200,
                Message = "تم جلب التقييمات بنجاح.",
                Data = new DoctorReviewsPageResultDto
                {
                    DoctorId = doctorId.Value,
                    IsEnabled = isEnabled,
                    AverageRating = averageRating,
                    ReviewCount = reviewCount,
                    TotalItems = reviewCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize,
                    Items = items
                }
            });
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
                review.AppointmentId == form.AppointmentId &&
                !review.IsDeleted))
            {
                return BadRequest("This booking has already been reviewed.");
            }

            var comment = form.Comment?.Trim() ?? string.Empty;
            if (ProfanityFilterServices.ContainsProfanity(comment))
            {
                return BadRequest("التعليق يحتوي على كلمات ممنوعة.");
            }

            var review = new Review
            {
                DoctorId = form.DoctorId,
                UserId = userId.Value,
                Rating = form.Rating,
                Comment = comment,
                AppointmentId = appointment.Id,
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
            var body = $"قام أحد المراجعين بتقييمك {review.Rating} من 5 بعد الحجز رقم {review.AppointmentId}.";
            var data = new Dictionary<string, string>
            {
                ["type"] = "review",
                ["reviewId"] = review.Id.ToString(),
                ["doctorId"] = review.DoctorId.ToString(),
                ["appointmentId"] = review.AppointmentId?.ToString() ?? string.Empty,
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
                appointmentId: review.AppointmentId);
            await _context.SaveChangesAsync();
        }

        private Task<bool> IsReviewFeatureEnabledAsync(int doctorId)
        {
            var now = BusinessClock.Now();
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

        private async Task<PaginationDto.PageResult<GetReviewDto>> GetReviewsPageAsync(int doctorId, int page, int pageSize)
        {
            var baseQuery = _context.Reviews
                .AsNoTracking()
                .Where(review => review.DoctorId == doctorId && !review.IsDeleted);

            var totalItems = await baseQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await baseQuery
                .OrderByDescending(review => review.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                    AppointmentId = review.AppointmentId,
                    Appointment = review.Appointment == null
                        ? null
                        : new GetAppointmentReview
                        {
                            Id = review.Appointment.Id,
                            Status = review.Appointment.Status
                        }
                })
                .ToListAsync();

            return new PaginationDto.PageResult<GetReviewDto>(items, totalItems, totalPages, page, pageSize);
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

        private static ResponseDto<string> BadRequestDto(string message) =>
            new() { Status = "Error", Code = 400, Message = message };

        private static ResponseDto<string> NotFoundDto(string message) =>
            new() { Status = "Error", Code = 404, Message = message };

        private static ResponseDto<string> UnauthorizedDto() =>
            new() { Status = "Error", Code = 401, Message = "You must sign in before adding a review." };

        private static IActionResult BadRequest(string message)
        {
            return new BadRequestObjectResult(BadRequestDto(message));
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
            return new NotFoundObjectResult(NotFoundDto(message));
        }

        private static IActionResult Unauthorized()
        {
            return new UnauthorizedObjectResult(UnauthorizedDto());
        }
    }
}
