using Clinic_Booking.DTOs.DoctorDTO;
using Clinic_Booking.DTOs.UserDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IDoctorServices
{
    public interface IDoctorServices
    {
        Task<ActionResult<PaginationDto.PageResult<GetDoctorDto>>> GetListAsync(SearchDoctorDto form, int page = 1, int pageSize = 10);
        Task<IActionResult> AddDoctorAsync(DoctorAddDto form);
        Task<IActionResult> GetMyProfileAsync();
        Task<IActionResult> UpdateMyProfileAsync(DoctorProfileUpdateDto form);
        Task<IActionResult> LinkAccountAsync(int doctorId, LinkDoctorAccountDto form);
        Task<IActionResult> UnlinkAccountAsync(int doctorId);
        Task<IActionResult> UpdateDoctorAsync(DoctorUpdateDto form);
        Task<IActionResult> DeleteAsync(int id);
    }
}
