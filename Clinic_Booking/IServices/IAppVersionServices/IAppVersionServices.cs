using Clinic_Booking.DTOs.AppVersionDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IAppVersionServices
{
    public interface IAppVersionServices
    {
        Task<IActionResult> GetListAsync();
        Task<IActionResult> CheckAsync(string platform, string currentVersion, int currentBuildNumber);
        Task<IActionResult> UpsertAsync(UpdateAppVersionPolicyDto dto);
    }
}
