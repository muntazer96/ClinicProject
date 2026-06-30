using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IFeatureServices
{
    public interface IFeatureServices
    {
        Task<IActionResult> GetItemsAsync();
    }
}
