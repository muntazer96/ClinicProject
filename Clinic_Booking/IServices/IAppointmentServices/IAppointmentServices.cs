using Clinic_Booking.DTOs.AppointmentDTO;
using Clinic_Booking.DTOs.UserDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IAppointmentServices
{
    public interface IAppointmentServices
    {
        Task<ActionResult<PaginationDto.PageResult<GetApponitmentDto>>> GetListAsync(SearchAppointmentDto form, int page = 1, int pageSize = 10);
        Task<IActionResult> GetAppointmentsAsync(SearchAppointmentDto form);
        Task<IActionResult> GetQueueAvailabilityAsync(int clinicId, DateOnly? fromDate, int days = 7);
        Task<IActionResult> GetGuestAppointmentAsync(string phoneNumber, string code);
        Task<IActionResult> GetMyAppointmentsAsync();
        Task<IActionResult> GetMyAppointmentAsync(int appointmentId);
        Task<IActionResult> CreateAppointmentAsync(AddAppointmentDto form);
        Task<IActionResult> ResendBookingOtpAsync(ResendBookingOtpDto form);
        Task<IActionResult> ConfirmBookingOtpAsync(BookingOtpDto form);
        Task<IActionResult> CancelGuestAppointmentAsync(CancelGuestAppointmentDto form);
        Task<IActionResult> CancelMyAppointmentAsync(CancelMyAppointmentDto form);
        Task<IActionResult> ToggleAppointmentStatusAsync(int appointmentId);
        Task<IActionResult> CompleteAppointmentAsync(int appointmentId);
    }
}
