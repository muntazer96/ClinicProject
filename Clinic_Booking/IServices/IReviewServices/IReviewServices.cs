using Clinic_Booking.DTOs.ReviewDTO;
using Clinic_Booking.DTOs.UserDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IReviewServices
{
    public interface IReviewServices
    {
        Task<ActionResult<PaginationDto.PageResult<GetReviewDto>>> GetByDoctorAsync(int doctorId, int page = 1, int pageSize = 10);
        Task<IActionResult> GetMineForDoctorAsync(int page = 1, int pageSize = 10);
        Task<IActionResult> AddAsync(AddReviewDto form);
    }
}
