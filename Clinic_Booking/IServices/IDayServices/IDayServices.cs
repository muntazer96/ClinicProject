using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IDayServices
{
    public interface IDayServices
    {
        Task<IActionResult> GetItemsAsync();
    }
}
