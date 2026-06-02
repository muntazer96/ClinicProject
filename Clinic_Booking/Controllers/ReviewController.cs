using Clinic_Booking.DTOs.ReviewDTO;
using Clinic_Booking.IServices.IReviewServices;
using Clinic_Booking.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> GetByDoctorAsync(int doctorId)
        {
            return await _services.GetByDoctorAsync(doctorId);
        }

        [HttpGet("doctor/my")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> GetMineForDoctorAsync()
        {
            return await _services.GetMineForDoctorAsync();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddAsync(AddReviewDto form)
        {
            return await _services.AddAsync(form);
        }
    }
}
