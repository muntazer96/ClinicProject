using Clinic_Booking.Authorization;
using Clinic_Booking.DTOs.DatabaseBuckupDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.User;
using Clinic_Booking.IServices.IDatabaseBackupServices;
using Clinic_Booking.IServices.ILoadServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_Booking.Controllers
{
    [Authorize]
    public class DatabaseBackupsController : BaseApiController
    {
        private readonly IDatabaseBackupService _service;
        private readonly ILoadServices _currentUser;
        private readonly UserManager<AspNetUsers> _userManager;

        public DatabaseBackupsController(
            IDatabaseBackupService service,
            ILoadServices currentUser,
            UserManager<AspNetUsers> userManager)
        {
            _service = service;
            _currentUser = currentUser;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<ActionResult<ResponseDto<List<DatabaseBackupResponse>>>> GetAll(
            CancellationToken ct = default)
        {
            var result = await _service.GetAllAsync(ct);


            return Ok(new ResponseDto<List<DatabaseBackupResponse>>
            {
                Status = "success",
                Code = 200,
                Message = "تم جلب النسخ الاحتياطية بنجاح.",
                Data = result
            });
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<ActionResult<ResponseDto<DatabaseBackupResponse>>> Create(
            CancellationToken ct = default)
        {
            var result = await _service.EnqueueAsync(
                _currentUser.GetCurrentUserId(),
                _currentUser.GetCurrentUsername(),
                ct: ct);

            return Accepted(new ResponseDto<DatabaseBackupResponse>
            {
                Status = "success",
                Code = 202,
                Message = "تمت إضافة النسخة الاحتياطية إلى قائمة التنفيذ.",
                Data = result
            });
        }

        [HttpPost("open-folder")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<ActionResult<ResponseDto<string>>> OpenFolder(
            CancellationToken ct = default)
        {
            await _service.OpenFolderAsync(ct);


            return Ok(new ResponseDto<string>
            {
                Status = "success",
                Code = 200,
                Message = "تم فتح مجلد النسخ الاحتياطية.",
                Data = null
            });
        }

        [HttpGet("restore-status")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<ActionResult<ResponseDto<DatabaseRestoreResponse?>>>
            GetRestoreStatus(CancellationToken ct = default)
        {
            var result = await _service.GetRestoreStatusAsync(ct);


            return Ok(new ResponseDto<DatabaseRestoreResponse?>
            {
                Status = "success",
                Code = 200,
                Message = "تم جلب حالة الاستعادة.",
                Data = result
            });
        }

        [HttpPost("restore")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<ActionResult<ResponseDto<DatabaseRestoreResponse>>> Restore(
            [FromBody] CreateDatabaseRestoreRequest request,
            CancellationToken ct = default)
        {

            var userId = _currentUser.GetCurrentUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(new ResponseDto<DatabaseRestoreResponse>
                {
                    Status = "error",
                    Code = 401,
                    Message = "يجب تسجيل الدخول لتنفيذ الاستعادة.",
                    Data = null
                });
            }

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());

            if (user == null ||
                string.IsNullOrWhiteSpace(request.Password) ||
                !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return BadRequest(new ResponseDto<DatabaseRestoreResponse>
                {
                    Status = "error",
                    Code = 400,
                    Message = "كلمة المرور غير صحيحة.",
                    Data = null
                });
            }

            var result = await _service.EnqueueRestoreAsync(
                request,
                userId,
                _currentUser.GetCurrentUsername(),
                ct);

            return Accepted(new ResponseDto<DatabaseRestoreResponse>
            {
                Status = "success",
                Code = 202,
                Message = "تمت إضافة عملية الاستعادة إلى قائمة التنفيذ.",
                Data = result
            });
        }

        [HttpGet("{id}/download")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<IActionResult> Download(
            string id,
            CancellationToken ct = default)
        {

            var backup = await _service.GetFileAsync(id, ct);

            if (backup == null)
            {
                return NotFound(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 404,
                    Message = "النسخة الاحتياطية غير موجودة أو غير مكتملة.",
                    Data = null
                });
            }

            return PhysicalFile(
                backup.Path,
                "application/octet-stream",
                backup.FileName);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AppRoles.SuperAdmin)]
        public async Task<ActionResult<ResponseDto<string>>> Delete(
            string id,
            CancellationToken ct = default)
        {

            var deleted = await _service.DeleteAsync(id, ct);

            if (!deleted)
            {
                return NotFound(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 404,
                    Message = "النسخة الاحتياطية غير موجودة.",
                    Data = null
                });
            }

            return Ok(new ResponseDto<string>
            {
                Status = "success",
                Code = 200,
                Message = "تم حذف النسخة الاحتياطية.",
                Data = null
            });
        }
    }
}
