using Clinic_Booking.DTOs.ReviewDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IReviewServices
{
    public interface IReviewServices
    {
        Task<IActionResult> GetByDoctorAsync(int doctorId);
        Task<IActionResult> GetMineForDoctorAsync();
        Task<IActionResult> AddAsync(AddReviewDto form);
    }
}
