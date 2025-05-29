using Clinic_Booking.DTOs.DoctorAvailabilityDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IDoctorAvailabilityServices
{
    public interface IDoctorAvailabilityServices
    {
        //Task<IActionResult> SetWeeklyAvailabilityAsync(AddDoctorAvailabilityDto dto);
        Task<IActionResult> UpsertWeeklyAvailabilityAsync(AddDoctorAvailabilityDto dto);
        Task<IActionResult> GetWeeklyAvailabilityAsync(int doctorId);
        Task<IActionResult> UpdateSingleDayAvailabilityAsync(UpdateSingleDayAvailabilityDto dto);
    }
}
