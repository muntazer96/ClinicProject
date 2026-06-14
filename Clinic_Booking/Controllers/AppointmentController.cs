using Clinic_Booking.DTOs.AppointmentDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.IAppointmentServices;
using Clinic_Booking.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Clinic_Booking.Controllers
{

    public class AppointmentController : BaseApiController
    {
        private readonly IAppointmentServices _service;
        public AppointmentController(IAppointmentServices service)
        {
            _service = service;
        }
        [HttpGet("GetListAsync")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<ActionResult<PaginationDto.PageResult<GetApponitmentDto>>> GetListAsync([FromQuery] SearchAppointmentDto form, int page = 1, int pageSize = 10)
        {
            return await _service.GetListAsync(form, page, pageSize);
        }
        [HttpGet]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetAppointmentsAsync([FromQuery] SearchAppointmentDto form)
        {
            return await _service.GetAppointmentsAsync(form);
        }
        [HttpGet("doctor/my")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> GetMineForDoctorAsync([FromQuery] SearchAppointmentDto form)
        {
            return await _service.GetMineForDoctorAsync(form);
        }
        [HttpGet("queue-availability/{clinicId}")]
        public async Task<IActionResult> GetQueueAvailabilityAsync(int clinicId, [FromQuery] DateOnly? fromDate, int days = 7)
        {
            return await _service.GetQueueAvailabilityAsync(clinicId, fromDate, days);
        }
        [HttpGet("guest")]
        public async Task<IActionResult> GetGuestAppointmentAsync([FromQuery] string phoneNumber, [FromQuery] string code)
        {
            return await _service.GetGuestAppointmentAsync(phoneNumber, code);
        }
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyAppointmentsAsync()
        {
            return await _service.GetMyAppointmentsAsync();
        }
        [HttpGet("my/{appointmentId}")]
        [Authorize]
        public async Task<IActionResult> GetMyAppointmentAsync(int appointmentId)
        {
            return await _service.GetMyAppointmentAsync(appointmentId);
        }
        [HttpPost]
        [EnableRateLimiting("Booking")]
        public async Task<IActionResult> CreateAppointmentAsync([FromBody] AddAppointmentDto form)
        {
            return await _service.CreateAppointmentAsync(form);
        }
        [HttpPost("manual")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> CreateManualAppointmentAsync([FromBody] ManualAppointmentDto form)
        {
            return await _service.CreateManualAppointmentAsync(form);
        }
        [HttpPost("otp/resend")]
        [EnableRateLimiting("Otp")]
        public async Task<IActionResult> ResendBookingOtpAsync([FromBody] ResendBookingOtpDto form)
        {
            return await _service.ResendBookingOtpAsync(form);
        }
        [HttpPost("otp/confirm")]
        [EnableRateLimiting("Otp")]
        public async Task<IActionResult> ConfirmBookingOtpAsync([FromBody] BookingOtpDto form)
        {
            return await _service.ConfirmBookingOtpAsync(form);
        }
        [HttpPost("guest/cancel")]
        public async Task<IActionResult> CancelGuestAppointmentAsync([FromBody] CancelGuestAppointmentDto form)
        {
            return await _service.CancelGuestAppointmentAsync(form);
        }
        [HttpPost("my/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelMyAppointmentAsync([FromBody] CancelMyAppointmentDto form)
        {
            return await _service.CancelMyAppointmentAsync(form);
        }
        [HttpPost("toggle-status")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> ToggleAppointmentStatusAsync([FromQuery] int appointmentId)
        {
            return await _service.ToggleAppointmentStatusAsync(appointmentId);
        }
        [HttpPost("complete")]
        [Authorize(Roles = AppRoles.DoctorUser)]
        public async Task<IActionResult> CompleteAppointmentAsync([FromQuery] int appointmentId)
        {
            return await _service.CompleteAppointmentAsync(appointmentId);
        }
    }
}
