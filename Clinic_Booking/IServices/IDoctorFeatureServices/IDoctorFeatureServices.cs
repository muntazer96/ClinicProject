using Clinic_Booking.DTOs.DoctorFeatureDTO;
using Clinic_Booking.DTOs.UserDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IDoctorFeatureServices
{
    public interface IDoctorFeatureServices
    {
        Task<ActionResult<PaginationDto.PageResult<GetDoctorFeatureDto>>> GetListAsync(int page = 1, int pageSize = 10);
    }
}
