using Clinic_Booking.DTOs.AppReleaseDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IAppReleaseServices
{
    public interface IAppReleaseServices
    {
        Task<IActionResult> UploadAsync(CreateAppReleaseDto dto);
        Task<IActionResult> GetLatestAsync();
        Task<IActionResult> DownloadAsync();
        Task<IActionResult> GetListAsync();
        Task<IActionResult> ToggleActiveAsync(int id);
        Task<IActionResult> DeleteAsync(int id);
    }
}
