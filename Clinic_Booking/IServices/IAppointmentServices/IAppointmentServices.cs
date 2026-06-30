using Clinic_Booking.DTOs.AppointmentDTO;
using Clinic_Booking.DTOs.UserDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IAppointmentServices
{
    public interface IAppointmentServices
    {
        Task<ActionResult<PaginationDto.PageResult<GetApponitmentDto>>> GetListAsync(SearchAppointmentDto form, int page = 1, int pageSize = 10);
        Task<IActionResult> GetAppointmentsAsync(SearchAppointmentDto form);
        Task<ActionResult<PaginationDto.PageResult<BookingDetailsDto>>> GetMineForDoctorAsync(SearchAppointmentDto form, int page = 1, int pageSize = 10);
        Task<IActionResult> GetQueueAvailabilityAsync(int clinicId, DateOnly? fromDate, int days = 7);
        Task<IActionResult> GetGuestAppointmentAsync(string phoneNumber, string code);
        Task<ActionResult<PaginationDto.PageResult<BookingDetailsDto>>> GetMyAppointmentsAsync(DateOnly? fromDate, DateOnly? toDate, int page = 1, int pageSize = 10);
        Task<IActionResult> GetMyAppointmentAsync(int appointmentId);
        Task<IActionResult> CreateAppointmentAsync(AddAppointmentDto form);
        Task<IActionResult> CreateManualAppointmentAsync(ManualAppointmentDto form);
        Task<IActionResult> ResendBookingOtpAsync(ResendBookingOtpDto form);
        Task<IActionResult> ConfirmBookingOtpAsync(BookingOtpDto form);
        Task<IActionResult> CancelGuestAppointmentAsync(CancelGuestAppointmentDto form);
        Task<IActionResult> CancelMyAppointmentAsync(CancelMyAppointmentDto form);
        Task<IActionResult> ToggleAppointmentStatusAsync(int appointmentId);
        Task<IActionResult> RejectPendingAppointmentAsync(int appointmentId);
        Task<IActionResult> CompleteAppointmentAsync(int appointmentId);
        Task<IActionResult> GetDoctorStatisticsAsync();
        Task<IActionResult> GetNextUpcomingAsync();
    }
}
