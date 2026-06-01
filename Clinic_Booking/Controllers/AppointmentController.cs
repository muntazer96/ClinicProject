using Clinic_Booking.DTOs.AppointmentDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.IAppointmentServices;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<PaginationDto.PageResult<GetApponitmentDto>>> GetListAsync([FromQuery] SearchAppointmentDto form, int page = 1, int pageSize = 10)
        {
            return await _service.GetListAsync(form, page, pageSize);
        }
        [HttpGet]
        public async Task<IActionResult> GetAppointmentsAsync([FromQuery] SearchAppointmentDto form)
        {
            return await _service.GetAppointmentsAsync(form);
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
        [HttpPost]
        public async Task<IActionResult> CreateAppointmentAsync(AddAppointmentDto form)
        {
            return await _service.CreateAppointmentAsync(form);
        }
        [HttpPost("guest/cancel")]
        public async Task<IActionResult> CancelGuestAppointmentAsync(CancelGuestAppointmentDto form)
        {
            return await _service.CancelGuestAppointmentAsync(form);
        }
        [HttpPost("my/cancel")]
        public async Task<IActionResult> CancelMyAppointmentAsync(CancelMyAppointmentDto form)
        {
            return await _service.CancelMyAppointmentAsync(form);
        }
        [HttpPost("toggle-status")]
        public async Task<IActionResult> ToggleAppointmentStatusAsync([FromQuery] int appointmentId)
        {
            return await _service.ToggleAppointmentStatusAsync(appointmentId);
        }
        [HttpPost("complete")]
        public async Task<IActionResult> CompleteAppointmentAsync(int appointmentId)
        {
            return await _service.CompleteAppointmentAsync(appointmentId);
        }
    }
}
