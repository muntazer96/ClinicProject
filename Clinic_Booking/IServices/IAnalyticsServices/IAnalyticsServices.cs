using Clinic_Booking.DTOs.AnalyticsDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IAnalyticsServices
{
    public interface IAnalyticsServices
    {
        Task<IActionResult> TrackAsync(TrackAnalyticsEventDto form);
        Task<IActionResult> GetAdminSummaryAsync(DateOnly? fromDate, DateOnly? toDate);
        Task<IActionResult> GetDoctorSummaryAsync(DateOnly? fromDate, DateOnly? toDate);
        Task<IActionResult> GetDoctorSummaryForAdminAsync(int doctorId, DateOnly? fromDate, DateOnly? toDate);
    }
}
