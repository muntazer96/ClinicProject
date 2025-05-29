using Clinic_Booking.DTOs.AppointmentDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IAppointmentServices
{
    public interface IAppointmentServices
    {
        Task<IActionResult> GetAppointmentsAsync(SearchAppointmentDto form);
        Task<IActionResult> CreateAppointmentAsync(AddAppointmentDto form);
        Task<IActionResult> ToggleAppointmentStatusAsync(int appointmentId);
        Task<IActionResult> CompleteAppointmentAsync(int appointmentId);
    }
}
