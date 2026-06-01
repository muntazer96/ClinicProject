using Clinic_Booking.Data;
using Clinic_Booking.DTOs.ClinicDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.Clinic;
using Clinic_Booking.Extensions;
using Clinic_Booking.IServices.IClinicServices;
using Clinic_Booking.IServices.ILoadServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.ClinicServices
{
    public class ClinicServices : IClinicServices
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoadServices _load;

        public ClinicServices(ApplicationDbContext context, ILoadServices load)
        {
            _context = context;
            _load = load;
        }

        public async Task<IActionResult> GetByDoctorAsync(int doctorId)
        {
            var doctorExists = await _context.Doctors
                .AnyAsync(d => d.Id == doctorId && !d.IsDeleted);
            if (!doctorExists)
            {
                return NotFound("الدكتور غير موجود.");
            }

            var clinics = await MapClinics(_context.Clinics
                .Where(c => c.DoctorId == doctorId && !c.IsDeleted && c.IsVisible))
                .ToListAsync();

            return Ok(clinics, "تم جلب عيادات الطبيب بنجاح.");
        }

        public async Task<IActionResult> GetMineAsync()
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (doctorId == null)
            {
                return Unauthorized();
            }

            var clinics = await MapClinics(_context.Clinics
                .Where(c => c.DoctorId == doctorId && !c.IsDeleted))
                .ToListAsync();

            return Ok(clinics, "تم جلب عياداتك بنجاح.");
        }

        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var clinic = await MapClinics(_context.Clinics
                .Where(c => c.Id == id && !c.IsDeleted && c.IsVisible && !c.Doctor.IsDeleted))
                .FirstOrDefaultAsync();

            return clinic == null
                ? NotFound("العيادة غير موجودة.")
                : Ok(clinic, "تم جلب معلومات العيادة بنجاح.");
        }

        public async Task<IActionResult> AddMineAsync(ClinicAddDto form)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (doctorId == null)
            {
                return Unauthorized();
            }

            var validation = Validate(form.Name, form.Address, form.Latitude, form.Longitude);
            if (validation != null)
            {
                return validation;
            }

            var duplicate = await _context.Clinics.AnyAsync(c =>
                c.DoctorId == doctorId &&
                !c.IsDeleted &&
                c.Name == form.Name &&
                c.Address == form.Address);
            if (duplicate)
            {
                return BadRequest("هذه العيادة مضافة مسبقاً.");
            }

            var clinic = new Clinic
            {
                DoctorId = doctorId.Value,
                Name = form.Name.Trim(),
                IraqiProvince = form.IraqiProvince,
                Address = form.Address.Trim(),
                Latitude = form.Latitude,
                Longitude = form.Longitude,
                MapUrl = form.MapUrl?.Trim(),
                PhoneNumber = form.PhoneNumber?.Trim(),
                IsVisible = form.IsVisible,
                CreatorId = _load.GetCurrentUserId()
            };

            _context.Clinics.Add(clinic);
            await _context.SaveChangesAsync();

            return Ok(new { clinic.Id }, "تمت إضافة العيادة بنجاح.");
        }

        public async Task<IActionResult> UpdateMineAsync(ClinicUpdateDto form)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (doctorId == null)
            {
                return Unauthorized();
            }

            var validation = Validate(form.Name, form.Address, form.Latitude, form.Longitude);
            if (validation != null)
            {
                return validation;
            }

            var clinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.Id == form.Id && c.DoctorId == doctorId && !c.IsDeleted);
            if (clinic == null)
            {
                return NotFound("العيادة غير موجودة أو لا تملك صلاحية تعديلها.");
            }

            var duplicate = await _context.Clinics.AnyAsync(c =>
                c.Id != form.Id &&
                c.DoctorId == doctorId &&
                !c.IsDeleted &&
                c.Name == form.Name &&
                c.Address == form.Address);
            if (duplicate)
            {
                return BadRequest("توجد عيادة أخرى بنفس الاسم والعنوان.");
            }

            clinic.Name = form.Name.Trim();
            clinic.IraqiProvince = form.IraqiProvince;
            clinic.Address = form.Address.Trim();
            clinic.Latitude = form.Latitude;
            clinic.Longitude = form.Longitude;
            clinic.MapUrl = form.MapUrl?.Trim();
            clinic.PhoneNumber = form.PhoneNumber?.Trim();
            clinic.IsVisible = form.IsVisible;
            clinic.ModifierId = _load.GetCurrentUserId();
            clinic.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok<object>(null, "تم تحديث معلومات العيادة بنجاح.");
        }

        public async Task<IActionResult> DeleteMineAsync(int id)
        {
            var doctorId = await GetCurrentDoctorIdAsync();
            if (doctorId == null)
            {
                return Unauthorized();
            }

            var clinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.Id == id && c.DoctorId == doctorId && !c.IsDeleted);
            if (clinic == null)
            {
                return NotFound("العيادة غير موجودة أو لا تملك صلاحية حذفها.");
            }

            clinic.IsDeleted = true;
            clinic.DeleterId = _load.GetCurrentUserId();
            clinic.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok<object>(null, "تم حذف العيادة بنجاح.");
        }

        private async Task<int?> GetCurrentDoctorIdAsync()
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return null;
            }

            return await _context.Doctors
                .Where(d => d.UserId == userId && !d.IsDeleted)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync();
        }

        private static IQueryable<GetClinicDto> MapClinics(IQueryable<Clinic> query)
        {
            return query
                .OrderBy(c => c.Id)
                .Select(c => new GetClinicDto
                {
                    Id = c.Id,
                    DoctorId = c.DoctorId,
                    Name = c.Name,
                    IraqiProvince = c.IraqiProvince,
                    IraqiProvinceName = c.IraqiProvince.GetDisplayName(),
                    Address = c.Address,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    MapUrl = c.MapUrl,
                    PhoneNumber = c.PhoneNumber,
                    IsVisible = c.IsVisible
                });
        }

        private static IActionResult? Validate(
            string name,
            string address,
            decimal? latitude,
            decimal? longitude)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
            {
                return BadRequest("اسم العيادة مطلوب ويجب ألا يتجاوز 200 حرف.");
            }

            if (string.IsNullOrWhiteSpace(address) || address.Length > 500)
            {
                return BadRequest("عنوان العيادة مطلوب ويجب ألا يتجاوز 500 حرف.");
            }

            if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
            {
                return BadRequest("إحداثيات موقع العيادة غير صحيحة.");
            }

            if ((latitude == null) != (longitude == null))
            {
                return BadRequest("يجب إدخال خط العرض وخط الطول معاً.");
            }

            return null;
        }

        private static IActionResult Ok<T>(T data, string message)
        {
            return new OkObjectResult(new ResponseDto<T>
            {
                Status = "Success",
                Code = 200,
                Message = message,
                Data = data
            });
        }

        private static IActionResult NotFound(string message)
        {
            return new NotFoundObjectResult(new ResponseDto<string>
            {
                Status = "Error",
                Code = 404,
                Message = message
            });
        }

        private static IActionResult BadRequest(string message)
        {
            return new BadRequestObjectResult(new ResponseDto<string>
            {
                Status = "Error",
                Code = 400,
                Message = message
            });
        }

        private static IActionResult Unauthorized()
        {
            return new UnauthorizedObjectResult(new ResponseDto<string>
            {
                Status = "Error",
                Code = 401,
                Message = "يجب تسجيل الدخول بحساب طبيب مرتبط."
            });
        }
    }
}
