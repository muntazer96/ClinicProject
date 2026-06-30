using Clinic_Booking.DTOs.DoctorOfferDTO;
using Clinic_Booking.DTOs.UserDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IDoctorOfferServices
{
    public interface IDoctorOfferServices
    {
        Task<ActionResult<PaginationDto.PageResult<DoctorOfferDto>>> GetListAsync(SearchDoctorOfferDto filter, int page = 1, int pageSize = 10);
        Task<ActionResult<PaginationDto.PageResult<DoctorOfferDto>>> GetMineAsync(SearchDoctorOfferDto filter, int page = 1, int pageSize = 10);
        Task<ActionResult<PaginationDto.PageResult<DoctorOfferDto>>> GetPublicAsync(SearchDoctorOfferDto filter, int page = 1, int pageSize = 10);
        Task<IActionResult> GetQuotaAsync(int doctorId);
        Task<IActionResult> GetMyQuotaAsync();
        Task<IActionResult> AddAsync(DoctorOfferUpsertDto form);
        Task<IActionResult> AddMineAsync(DoctorOfferUpsertDto form);
        Task<IActionResult> UpdateAsync(DoctorOfferUpsertDto form);
        Task<IActionResult> UpdateMineAsync(DoctorOfferUpsertDto form);
        Task<IActionResult> DeleteAsync(int id);
        Task<IActionResult> DeleteMineAsync(int id);
    }
}
