using Clinic_Booking.Authorization;
using Clinic_Booking.DTOs.DoctorRequestDTO;
using Clinic_Booking.IServices.IDoctorRequestServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Clinic_Booking.Controllers
{
    public class DoctorRequestController : BaseApiController
    {
        private readonly IDoctorRequestServices _service;

        public DoctorRequestController(IDoctorRequestServices service)
        {
            _service = service;
        }

        [HttpPost("check-phone")]
        [AllowAnonymous]
        [EnableRateLimiting("Auth")]
        public async Task<IActionResult> CheckPhone([FromBody] CheckPhoneRequestDto form)
        {
            return await _service.CheckPhoneAsync(form);
        }

        [HttpPost("send-otp")]
        [AllowAnonymous]
        [EnableRateLimiting("Otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendDoctorRequestOtpDto form)
        {
            return await _service.SendOtpAsync(form);
        }

        [HttpPost("verify-otp")]
        [AllowAnonymous]
        [EnableRateLimiting("Otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyDoctorRequestOtpDto form)
        {
            return await _service.VerifyOtpAsync(form);
        }

        [HttpPost("submit")]
        [AllowAnonymous]
        [EnableRateLimiting("DoctorRequestSubmit")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> Submit([FromForm] CreateDoctorRequestDto form)
        {
            return await _service.CreateRequestAsync(form);
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] string? search = null)
        {
            return await _service.GetAllAsync(page, pageSize, status, search);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> GetById(int id)
        {
            return await _service.GetByIdAsync(id);
        }

        [HttpGet("by-code/{code}")]
        [AllowAnonymous]
        [EnableRateLimiting("AccountRecovery")]
        public async Task<IActionResult> GetByCode(string code, [FromQuery] string phoneNumber)
        {
            return await _service.GetByCodeAsync(code, phoneNumber);
        }

        [HttpPost("{id}/accept")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> Accept(int id)
        {
            return await _service.AcceptAsync(new AcceptRequestDto { RequestId = id });
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectRequestDto form)
        {
            form.RequestId = id;
            return await _service.RejectAsync(form);
        }
    }
}
