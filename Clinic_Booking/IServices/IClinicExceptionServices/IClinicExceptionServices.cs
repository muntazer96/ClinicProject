using Clinic_Booking.DTOs.ClinicExceptionDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IClinicExceptionServices
{
    public interface IClinicExceptionServices
    {
        Task<IActionResult> GetMineAsync(int clinicId, DateOnly? fromDate, DateOnly? toDate);
        Task<IActionResult> UpsertMineAsync(UpsertClinicExceptionDto form);
        Task<IActionResult> DeleteMineAsync(int id);
    }
}
