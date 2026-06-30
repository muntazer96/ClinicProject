using Clinic_Booking.Data;
using Clinic_Booking.DTOs.DoctorRequestDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.Doctor;
using Clinic_Booking.Entities.RequestFrom;
using Clinic_Booking.Entities.User;
using Clinic_Booking.Entities.UserPhoneOtpRequest;
using Clinic_Booking.Enums;
using Clinic_Booking.IServices.IDoctorRequestServices;
using Clinic_Booking.IServices.IWhatsAppMessageServices;
using Clinic_Booking.IServices.ITelegramAlertService;
using Clinic_Booking.Authorization;
using Clinic_Booking.Entities.Role;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Clinic_Booking.Extensions;

namespace Clinic_Booking.Services.DoctorRequestServices
{
    public class DoctorRequestServices : IDoctorRequestServices
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AspNetUsers> _userManager;
        private readonly RoleManager<AspNetRoles> _roleManager;
        private readonly IWebHostEnvironment _environment;
        private readonly IWhatsAppMessageServices _whatsApp;
        private readonly ILogger<DoctorRequestServices> _logger;
        private readonly ITelegramAlertService _telegramAlert;

        private static readonly TimeSpan OtpExpiration = TimeSpan.FromMinutes(5);
        private const int MaxOtpAttempts = 5;
        private const long MaxFileSize = 5 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public DoctorRequestServices(
            ApplicationDbContext context,
            UserManager<AspNetUsers> userManager,
            RoleManager<AspNetRoles> roleManager,
            IWebHostEnvironment environment,
            IWhatsAppMessageServices whatsApp,
            ILogger<DoctorRequestServices> logger,
            ITelegramAlertService telegramAlert)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _environment = environment;
            _whatsApp = whatsApp;
            _logger = logger;
            _telegramAlert = telegramAlert;
        }

        public async Task<IActionResult> CheckPhoneAsync(CheckPhoneRequestDto form)
        {
            //if (!await ValidateCaptchaAsync(form.CaptchaToken))
            //{
            //    return new BadRequestObjectResult(new ResponseDto<string>
            //    {
            //        Status = "error",
            //        Code = 400,
            //        Message = "فشل التحقق من Captcha. يرجى المحاولة مرة أخرى.",
            //        Data = null
            //    });
            //}

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == form.PhoneNumber && !u.IsDeleted);

            if (user == null)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 400,
                    Message = "رقم الهاتف غير مسجل. يرجى تحميل التطبيق وإنشاء حساب أولاً.",
                    Data = null
                });
            }

            return new OkObjectResult(new ResponseDto<CheckPhoneResponseDto>
            {
                Status = "success",
                Code = 200,
                Message = "تم التحقق من رقم الهاتف بنجاح.",
                Data = new CheckPhoneResponseDto
                {
                    UserId = user.Id,
                    PhoneNumber = user.PhoneNumber!
                }
            });
        }

        public async Task<IActionResult> SendOtpAsync(SendDoctorRequestOtpDto form)
        {
            var user = await _userManager.FindByIdAsync(form.UserId.ToString());
            if (user == null || user.PhoneNumber != form.PhoneNumber)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 400,
                    Message = "بيانات المستخدم غير صحيحة.",
                    Data = null
                });
            }

            var code = GenerateOtpCode();
            var salt = GenerateSalt();
            var codeHash = HashCode(code, salt);

            var otpRequest = new UserPhoneOtpRequest
            {
                UserId = user.Id,
                PhoneNumber = form.PhoneNumber,
                CodeHash = codeHash,
                CodeSalt = salt,
                ExpiresAt = BusinessClock.Now().Add(OtpExpiration),
                SentAt = BusinessClock.Now(),
                AttemptCount = 0,
                IsUsed = false
            };

            _context.UserPhoneOtpRequests.Add(otpRequest);
            await _context.SaveChangesAsync();

            var otpSent = await SendOtpViaWhatsApp(form.PhoneNumber, code);

            if (!otpSent)
            {
                _logger.LogWarning("Doctor request OTP failed for {PhoneNumber}. Telegram alert sent.", form.PhoneNumber);
            }

            _logger.LogInformation("OTP sent to {PhoneNumber} for doctor request (User: {UserId})", form.PhoneNumber, form.UserId);

            return new OkObjectResult(new ResponseDto<string>
            {
                Status = "success",
                Code = 200,
                Message = "تم إرسال رمز التحقق إلى رقم الهاتف.",
                Data = null
            });
        }

        public async Task<IActionResult> VerifyOtpAsync(VerifyDoctorRequestOtpDto form)
        {
            var otpRequest = await _context.UserPhoneOtpRequests
                .Where(o => o.UserId == form.UserId
                            && o.PhoneNumber == form.PhoneNumber
                            && !o.IsUsed
                            && o.ExpiresAt > BusinessClock.Now())
                .OrderByDescending(o => o.SentAt)
                .FirstOrDefaultAsync();

            if (otpRequest == null)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 400,
                    Message = "رمز التحقق غير صالح أو منتهي الصلاحية.",
                    Data = null
                });
            }

            if (otpRequest.AttemptCount >= MaxOtpAttempts)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 400,
                    Message = "لقد تجاوزت عدد المحاولات المسموح بها. يرجى طلب رمز جديد.",
                    Data = null
                });
            }

            otpRequest.AttemptCount++;

            if (otpRequest.CodeHash != HashCode(form.OtpCode, otpRequest.CodeSalt))
            {
                await _context.SaveChangesAsync();
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 400,
                    Message = "رمز التحقق غير صحيح.",
                    Data = null
                });
            }

            otpRequest.IsUsed = true;
            otpRequest.VerifiedAt = BusinessClock.Now();
            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<VerifyOtpResponseDto>
            {
                Status = "success",
                Code = 200,
                Message = "تم التحقق بنجاح.",
                Data = new VerifyOtpResponseDto
                {
                    VerificationTokenId = otpRequest.Id,
                    UserId = form.UserId,
                    PhoneNumber = form.PhoneNumber
                }
            });
        }

        public async Task<IActionResult> CreateRequestAsync(CreateDoctorRequestDto form)
        {
            var otpRequest = await _context.UserPhoneOtpRequests
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == form.VerificationTokenId);

            if (otpRequest == null || !otpRequest.IsUsed)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 400,
                    Message = "رمز التحقق غير صالح. يرجى إكمال التحقق من رقم الهاتف أولاً.",
                    Data = null
                });
            }

            if (otpRequest.VerifiedAt.HasValue &&
                BusinessClock.Now() - otpRequest.VerifiedAt.Value > TimeSpan.FromMinutes(30))
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 400,
                    Message = "انتهت صلاحية الجلسة. يرجى إعادة التحقق من رقم الهاتف.",
                    Data = null
                });
            }

            var existingPending = await _context.Set<RequestForm>()
                .AnyAsync(r => r.UserId == otpRequest.UserId
                               && r.RequestStatus == RequestStatus.Waiting
                               && !r.IsDeleted);

            if (existingPending)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 400,
                    Message = "لديك طلب قيد الانتظار بالفعل. يرجى انتظار مراجعته.",
                    Data = null
                });
            }

            var specialization = await _context.Specializations
                .FirstOrDefaultAsync(s => s.Id == form.SpecializationId);

            if (specialization == null)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 400,
                    Message = "التخصص المحدد غير موجود.",
                    Data = null
                });
            }

            var identityFrontPath = await SaveFileAsync(form.IdentityFront, "RequestIdentity");
            var identityBackPath = form.IdentityBack != null
                ? await SaveFileAsync(form.IdentityBack, "RequestIdentity")
                : null;

            var code = await GenerateRequestCodeAsync();

            var requestForm = new RequestForm
            {
                UserId = otpRequest.UserId,
                PhoneNumber = otpRequest.PhoneNumber,
                FullName = form.FullName,
                KnownName = form.KnownName,
                IraqiProvince = form.Province,
                BirthDay = form.BirthDay,
                Specialization = specialization,
                IdentityFront = identityFrontPath,
                IdentityBack = identityBackPath,
                RequestStatus = RequestStatus.Waiting,
                Code = code
            };

            _context.Set<RequestForm>().Add(requestForm);
            await _context.SaveChangesAsync();

            var message = $"تم استلام طلبك بنجاح وسوف يتم مراجعته من الإدارة.\nكود متابعة الطلب: {code}";
            await _whatsApp.SendMessageAsync(otpRequest.PhoneNumber, message);

            await _telegramAlert.SendNewDoctorRequestAlertAsync(
                requestForm.FullName,
                requestForm.PhoneNumber,
                requestForm.IraqiProvince.GetDisplayName(),
                requestForm.KnownName);

            return new OkObjectResult(new ResponseDto<DoctorRequestResponseDto>
            {
                Status = "success",
                Code = 200,
                Message = "تم إرسال طلب التحويل بنجاح.",
                Data = new DoctorRequestResponseDto
                {
                    Id = requestForm.Id,
                    Code = requestForm.Code,
                    Status = "Waiting",
                    CreatedAt = requestForm.CreatedAt ?? BusinessClock.Now(),
                    Message = message
                }
            });
        }

        public async Task<IActionResult> GetAllAsync(int page, int pageSize, string? status, string? search)
        {
            var query = _context.Set<RequestForm>()
                .Include(r => r.Specialization)
                .Where(r => !r.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<RequestStatus>(status, ignoreCase: true, out var statusFilter))
            {
                query = query.Where(r => r.RequestStatus == statusFilter);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r =>
                    r.FullName.Contains(search) ||
                    r.PhoneNumber.Contains(search) ||
                    r.Code.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new DoctorRequestListItemDto
                {
                    Id = r.Id,
                    Code = r.Code,
                    FullName = r.FullName,
                    KnownName = r.KnownName,
                    PhoneNumber = r.PhoneNumber,
                    Status = r.RequestStatus.ToString(),
                    SpecializationName = r.Specialization.Name,
                    ProvinceName = r.IraqiProvince.GetDisplayName(),
                    CreatedAt = r.CreatedAt ?? DateTime.MinValue,
                    RejectedReason = r.RejectedReason
                })
                .ToListAsync();

            return new OkObjectResult(new ResponseDto<DoctorRequestPaginationDto>
            {
                Status = "success",
                Code = 200,
                Message = "تم جلب الطلبات بنجاح.",
                Data = new DoctorRequestPaginationDto
                {
                    Items = items,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                }
            });
        }

        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var request = await _context.Set<RequestForm>()
                .Include(r => r.Specialization)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (request == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 404,
                    Message = "الطلب غير موجود.",
                    Data = null
                });
            }

            return new OkObjectResult(new ResponseDto<DoctorRequestDetailsDto>
            {
                Status = "success",
                Code = 200,
                Message = "تم جلب تفاصيل الطلب.",
                Data = new DoctorRequestDetailsDto
                {
                    Id = request.Id,
                    Code = request.Code,
                    UserId = request.UserId?.ToString(),
                    PhoneNumber = request.PhoneNumber,
                    FullName = request.FullName,
                    KnownName = request.KnownName,
                    Province = request.IraqiProvince.GetDisplayName(),
                    BirthDay = request.BirthDay,
                    SpecializationId = request.Specialization.Id,
                    SpecializationName = request.Specialization.Name,
                    IdentityFront = request.IdentityFront,
                    IdentityBack = request.IdentityBack,
                    Status = request.RequestStatus.ToString(),
                    RejectedReason = request.RejectedReason,
                    CreatedAt = request.CreatedAt ?? DateTime.MinValue,
                    ModifiedAt = request.ModifiedAt
                }
            });
        }

        public async Task<IActionResult> GetByCodeAsync(string code, string phoneNumber)
        {
            var request = await _context.Set<RequestForm>()
                .Include(r => r.Specialization)
                .FirstOrDefaultAsync(r => r.Code == code && r.PhoneNumber == phoneNumber && !r.IsDeleted);

            if (request == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 404,
                    Message = "الطلب غير موجود.",
                    Data = null
                });
            }

            return new OkObjectResult(new ResponseDto<DoctorRequestDetailsDto>
            {
                Status = "success",
                Code = 200,
                Message = "تم جلب حالة الطلب.",
                Data = new DoctorRequestDetailsDto
                {
                    Id = request.Id,
                    Code = request.Code,
                    PhoneNumber = request.PhoneNumber,
                    FullName = request.FullName,
                    KnownName = request.KnownName,
                    Province = request.IraqiProvince.GetDisplayName(),
                    BirthDay = request.BirthDay,
                    SpecializationId = request.Specialization.Id,
                    SpecializationName = request.Specialization.Name,
                    IdentityFront = request.IdentityFront,
                    IdentityBack = request.IdentityBack,
                    Status = request.RequestStatus.ToString(),
                    RejectedReason = request.RejectedReason,
                    CreatedAt = request.CreatedAt ?? DateTime.MinValue,
                    ModifiedAt = request.ModifiedAt
                }
            });
        }

        public async Task<IActionResult> AcceptAsync(AcceptRequestDto form)
        {
            var request = await _context.Set<RequestForm>()
                .Include(r => r.Specialization)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == form.RequestId && !r.IsDeleted);

            if (request == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 404,
                    Message = "الطلب غير موجود.",
                    Data = null
                });
            }

            if (request.RequestStatus != RequestStatus.Waiting)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 400,
                    Message = "يمكن قبول الطلبات قيد الانتظار فقط.",
                    Data = null
                });
            }

            if (request.User == null)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 400,
                    Message = "المستخدم المرتبط بالطلب غير موجود.",
                    Data = null
                });
            }

            request.RequestStatus = RequestStatus.Accepted;
            request.ModifiedAt = BusinessClock.Now();

            if (!await _userManager.IsInRoleAsync(request.User, AppRoles.DoctorUser))
            {
                await _userManager.AddToRoleAsync(request.User, AppRoles.DoctorUser);
            }

            var existingDoctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == request.UserId && !d.IsDeleted);

            if (existingDoctor == null)
            {
                var doctor = new Doctor
                {
                    Name = request.FullName,
                    NormalizedName = request.FullName.ToUpperInvariant(),
                    SpecializationId = request.Specialization.Id,
                    Specialization = request.Specialization,
                    Description = "",
                    SubscriptionRank = 0,
                    IraqiProvince = request.IraqiProvince,
                    ImageName = "default-doctor.png",
                    BirthDay = request.BirthDay,
                    PhoneNumber = request.PhoneNumber,
                    UserId = request.UserId,
                    IsPubliclyVisible = false
                };

                _context.Doctors.Add(doctor);

                var basicPackage = await _context.SubscriptionPackages
                    .OrderBy(p => p.Price)
                    .FirstOrDefaultAsync();

                if (basicPackage != null)
                {
                    var subscription = new Entities.DoctorSubscription.DoctorSubscription
                    {
                        Doctor = doctor,
                        PackageId = basicPackage.Id,
                        StartDate = BusinessClock.Now(),
                        EndDate = BusinessClock.Now().AddYears(1),
                        Status = SubscriptionStatus.Active
                    };
                    _context.DoctorSubscriptions.Add(subscription);
                }
            }

            await _context.SaveChangesAsync();

            var message = "تم قبول طلبك، يرجى تسجيل الدخول من جديد وإكمال معلومات الطبيب.";
            await _whatsApp.SendMessageAsync(request.PhoneNumber, message);

            return new OkObjectResult(new ResponseDto<string>
            {
                Status = "success",
                Code = 200,
                Message = "تم قبول الطلب بنجاح.",
                Data = message
            });
        }

        public async Task<IActionResult> RejectAsync(RejectRequestDto form)
        {
            var request = await _context.Set<RequestForm>()
                .FirstOrDefaultAsync(r => r.Id == form.RequestId && !r.IsDeleted);

            if (request == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 404,
                    Message = "الطلب غير موجود.",
                    Data = null
                });
            }

            if (request.RequestStatus != RequestStatus.Waiting)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "error",
                    Code = 400,
                    Message = "يمكن رفض الطلبات قيد الانتظار فقط.",
                    Data = null
                });
            }

            request.RequestStatus = RequestStatus.Rejected;
            request.RejectedReason = form.RejectedReason;
            request.ModifiedAt = BusinessClock.Now();
            await _context.SaveChangesAsync();

            var message = $"تم رفض طلبك بسبب: {form.RejectedReason}";
            await _whatsApp.SendMessageAsync(request.PhoneNumber, message);

            return new OkObjectResult(new ResponseDto<string>
            {
                Status = "success",
                Code = 200,
                Message = "تم رفض الطلب بنجاح.",
                Data = message
            });
        }

        private static string GenerateOtpCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        }

        private static string GenerateSalt()
        {
            var bytes = RandomNumberGenerator.GetBytes(16);
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static string HashCode(string code, string salt)
        {
            var input = code + salt;
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        //private async Task<bool> ValidateCaptchaAsync(string captchaToken)
        //{
        //    if (string.IsNullOrWhiteSpace(captchaToken))
        //        return false;

        //    if (_environment.IsDevelopment())
        //        return true;

        //    try
        //    {
        //        var httpClient = new HttpClient();
        //        var secretKey = "6Ld6YT0tAAAAAPBelhBY_J25MXYY6huHqI9N9fNh";
        //        var response = await httpClient.PostAsync(
        //            $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={captchaToken}",
        //            null);

        //        if (!response.IsSuccessStatusCode)
        //            return false;

        //        var json = await response.Content.ReadAsStringAsync();
        //        var result = System.Text.Json.JsonSerializer.Deserialize<RecaptchaResponse>(json);
        //        return result?.success == true;
        //    }
        //    catch
        //    {
        //        _logger.LogWarning("Captcha verification failed for token: {Token}", captchaToken);
        //        return false;
        //    }
        //}

        private async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException("نوع الملف غير مدعوم. الامتدادات المسموح بها: .jpg, .jpeg, .png, .webp");

            if (file.Length > MaxFileSize)
                throw new InvalidOperationException("حجم الملف يتجاوز 5 ميجابايت.");

            var uploadsFolder = Path.Combine(_environment.WebRootPath, subFolder);
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{subFolder}/{fileName}";
        }

        private async Task<string> GenerateRequestCodeAsync()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var randomBytes = RandomNumberGenerator.GetBytes(6);
            var code = new StringBuilder("DR-");
            for (int i = 0; i < 6; i++)
            {
                code.Append(chars[randomBytes[i] % chars.Length]);
            }

            var exists = await _context.Set<RequestForm>()
                .AnyAsync(r => r.Code == code.ToString());

            return exists ? await GenerateRequestCodeAsync() : code.ToString();
        }

        private async Task<bool> SendOtpViaWhatsApp(string phoneNumber, string code)
        {
            var message = $"رمز التحقق الخاص بك: {code}\nينتهي الصلاحية بعد 5 دقائق.";
            try
            {
                var sent = await _whatsApp.SendMessageAsync(phoneNumber, message);

                if (!sent)
                {
                    _logger.LogWarning("Doctor request OTP send failed for {PhoneNumber}.", phoneNumber);
                    await _telegramAlert.SendOtpFailureAlertAsync(
                        phoneNumber,
                        code,
                        message,
                        "WhatsApp message delivery failed");
                }

                return sent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Doctor request OTP send threw exception for {PhoneNumber}.", phoneNumber);
                await _telegramAlert.SendOtpFailureAlertAsync(
                    phoneNumber,
                    code,
                    message,
                    ex.Message);
                return false;
            }
        }

        private class RecaptchaResponse
        {
            public bool success { get; set; }
            public string? challenge_ts { get; set; }
            public string? hostname { get; set; }
        }
    }
}
