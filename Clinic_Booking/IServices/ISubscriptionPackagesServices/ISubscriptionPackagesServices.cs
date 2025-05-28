using Clinic_Booking.DTOs.SubscriptionPackagesDTO;
using Clinic_Booking.DTOs.UserDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.ISubscriptionPackagesServices
{
    public interface ISubscriptionPackagesServices
    {
        Task<ActionResult<PaginationDto.PageResult<GetSubscriptionPackages>>> GetListAsync(int page = 1, int pageSize = 10);
    }
}
