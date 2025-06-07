using Clinic_Booking.DTOs.AppointmentDTO;
using Clinic_Booking.DTOs.UserDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IAppointmentServices
{
    public interface IAppointmentServices
    {
        Task<ActionResult<PaginationDto.PageResult<GetApponitmentDto>>> GetListAsync(SearchAppointmentDto form, int page = 1, int pageSize = 10);
        Task<IActionResult> GetAppointmentsAsync(SearchAppointmentDto form);
        Task<IActionResult> CreateAppointmentAsync(AddAppointmentDto form);
        Task<IActionResult> ToggleAppointmentStatusAsync(int appointmentId);
        Task<IActionResult> CompleteAppointmentAsync(int appointmentId);
    }
}
