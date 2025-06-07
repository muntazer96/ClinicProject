using Clinic_Booking.Data;
using Clinic_Booking.DTOs.ReviewDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.Review;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.IReviewServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.ReviewServices
{
    public class ReviewServices : IReviewServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;
        public ReviewServices(ApplicationDbContext context , ILoadServices load) 
        { 
            _context = context;
            _load = load;
        }
        public async Task<IActionResult> AddAsync(AddReviewDto form)
        {
            try
            {
                var userId = _load.GetCurrentUserId();
                var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a =>
                a.Id == form.AppointmentId &&
                a.DoctorId == form.DoctorId &&
                a.UserId == userId &&
                a.Status == AppointmentStatus.Completed);

                if (appointment == null)
                {
                    return new BadRequestObjectResult(new ResponseDto<object>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "لا يمكن تقييم الدكتور إلا بعد إكمال موعد مؤكد معه.",
                        Data = null
                    });
                }

                // تحقق هل تم تقييم هذا الموعد مسبقاً
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.AppoinmentId == form.AppointmentId && r.UserId == userId);

                if (existingReview != null)
                {
                    return new BadRequestObjectResult(new ResponseDto<object>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "لقد قمت مسبقاً بتقييم هذا الموعد.",
                        Data = null
                    });
                }

                var review = new Review
                {
                    DoctorId = form.DoctorId,
                    UserId = (Guid)userId,
                    Rating = form.Rating,
                    Comment = form.Comment,
                    AppoinmentId = form.AppointmentId
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return new OkObjectResult(new ResponseDto<object>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "تم إضافة التقييم بنجاح.",
                    Data = new { ReviewId = review.Id }
                });

            }
            catch (Exception ex)
            {
                return new ObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "حدث خطأ غير متوقع!",
                    Data = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }
    }
}
