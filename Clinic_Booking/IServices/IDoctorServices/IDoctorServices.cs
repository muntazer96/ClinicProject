using Clinic_Booking.DTOs.DoctorDTO;
using Clinic_Booking.DTOs.UserDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IDoctorServices
{
    public interface IDoctorServices
    {
        Task<ActionResult<PaginationDto.PageResult<GetDoctorDto>>> GetListAsync(SearchDoctorDto form, int page = 1, int pageSize = 10);
        Task<IActionResult> AddDoctorAsync(DoctorAddDto form);
        Task<IActionResult> DeleteAsync(int id);
    }
}
