using Clinic_Booking.Data;
using Clinic_Booking.IServices.ILoadServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Controllers
{
    [Authorize]
    public class FilesController : BaseApiController
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILoadServices _load;
        private readonly ApplicationDbContext _context;

        public FilesController(IWebHostEnvironment env, ILoadServices load, ApplicationDbContext context)
        {
            _env = env;
            _load = load;
            _context = context;
        }

        [HttpGet("message-image/{fileName}")]
        public async Task<IActionResult> GetMessageImage(string fileName)
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
                return Unauthorized();

            var safeName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(safeName))
                return NotFound();

            var message = await _context.Messages
                .Where(m => m.ImageName == safeName)
                .Select(m => new { m.SenderId, m.ReceiverId })
                .FirstOrDefaultAsync();

            if (message == null)
                return NotFound();

            if (message.SenderId != userId && message.ReceiverId != userId)
                return Forbid();

            var path = Path.Combine(_env.WebRootPath, "MessageImages", safeName);
            if (!System.IO.File.Exists(path))
                return NotFound();

            return PhysicalFile(path, GetContentType(safeName));
        }

        [HttpGet("doctor-image/{fileName}")]
        public IActionResult GetDoctorImage(string fileName)
        {
            var safeName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(safeName))
                return NotFound();

            var path = Path.Combine(_env.WebRootPath, "DoctorImage", safeName);
            if (!System.IO.File.Exists(path))
                return NotFound();

            return PhysicalFile(path, GetContentType(safeName));
        }

        [HttpGet("user-profile-image/{fileName}")]
        public IActionResult GetUserProfileImage(string fileName)
        {
            var safeName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(safeName))
                return NotFound();

            var path = Path.Combine(_env.WebRootPath, "UserImgProfile", safeName);
            if (!System.IO.File.Exists(path))
                return NotFound();

            return PhysicalFile(path, GetContentType(safeName));
        }

        [HttpGet("app-download/{fileName}")]
        public IActionResult GetAppDownload(string fileName)
        {
            var safeName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(safeName))
                return NotFound();

            var path = Path.Combine(_env.WebRootPath, "AppDownloads", safeName);
            if (!System.IO.File.Exists(path))
                return NotFound();

            var ext = Path.GetExtension(safeName).ToLowerInvariant();
            var contentType = ext == ".apk" ? "application/vnd.android.package-archive" : "application/octet-stream";
            return PhysicalFile(path, contentType);
        }

        private static string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }
    }
}
