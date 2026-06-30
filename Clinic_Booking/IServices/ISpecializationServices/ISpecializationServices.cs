using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.ISpecializationServices
{
    public interface ISpecializationServices
    {
        Task<IActionResult> GetItemsAsync();
    }
}
