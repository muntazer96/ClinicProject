using Clinic_Booking.DTOs.ReviewDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.IReviewServices;
using Clinic_Booking.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Clinic_Booking.Controllers
{
    public class ReviewController : BaseApiController
    {
        private readonly IReviewServices _services;

        public ReviewController(IReviewServices services)
        {
            _services = services;
        }

        [HttpGet("doctor/{doctorId}")]
        [EnableRateLimiting("PublicRead")]
        public async Task<ActionResult<PaginationDto.PageResult<GetReviewDto>>> GetByDoctorAsync(
            int doctorId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            return await _services.GetByDoctorAsync(doctorId, page, pageSize);
        }

        [HttpGet("doctor/my")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> GetMineForDoctorAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            return await _services.GetMineForDoctorAsync(page, pageSize);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddAsync(AddReviewDto form)
        {
            return await _services.AddAsync(form);
        }
    }
}
