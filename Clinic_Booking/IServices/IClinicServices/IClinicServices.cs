using Clinic_Booking.DTOs.ClinicDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IClinicServices
{
    public interface IClinicServices
    {
        Task<IActionResult> GetByDoctorAsync(int doctorId);
        Task<IActionResult> GetByDoctorForAdminAsync(int doctorId);
        Task<IActionResult> GetMineAsync();
        Task<IActionResult> GetByIdAsync(int id);
        Task<IActionResult> AddMineAsync(ClinicAddDto form);
        Task<IActionResult> UpdateMineAsync(ClinicUpdateDto form);
        Task<IActionResult> DeleteMineAsync(int id);
    }
}
