using Clinic_Booking.DTOs.DoctorRequestDTO;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.IServices.IDoctorRequestServices
{
    public interface IDoctorRequestServices
    {
        Task<IActionResult> CheckPhoneAsync(CheckPhoneRequestDto form);
        Task<IActionResult> SendOtpAsync(SendDoctorRequestOtpDto form);
        Task<IActionResult> VerifyOtpAsync(VerifyDoctorRequestOtpDto form);
        Task<IActionResult> CreateRequestAsync(CreateDoctorRequestDto form);
        Task<IActionResult> GetAllAsync(int page, int pageSize, string? status, string? search);
        Task<IActionResult> GetByIdAsync(int id);
        Task<IActionResult> GetByCodeAsync(string code, string phoneNumber);
        Task<IActionResult> AcceptAsync(AcceptRequestDto form);
        Task<IActionResult> RejectAsync(RejectRequestDto form);
    }
}
