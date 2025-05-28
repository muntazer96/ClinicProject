using Clinic_Booking.DTOs.DoctorDTO;
using Clinic_Booking.DTOs.DoctorSubscriptionDTO;
using Clinic_Booking.DTOs.UserDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IDoctorSubscriptionServices
{
    public interface IDoctorSubscriptionServices
    {
        Task<ActionResult<PaginationDto.PageResult<GetDoctorSubscriptionDto>>> GetListAsync(SearchDoctorSubscriptionDto form,int page = 1, int pageSize = 10);
        Task<IActionResult> CreateSubscriptionAsync(DoctorSubscriptionAddDto form);
    }
}
