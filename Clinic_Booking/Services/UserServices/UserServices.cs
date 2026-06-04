using Clinic_Booking.Data;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.RefreshToken;
using Clinic_Booking.Entities.Role;
using Clinic_Booking.Entities.User;
using Clinic_Booking.Entities.UserPhoneOtpRequest;
using Clinic_Booking.IServices.IBookingSmsServices;
using Clinic_Booking.IServices.IEmailServices;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.IUserServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;

namespace Clinic_Booking.Services.UserServices
{
    public class UserServices : IUserServices
    {
        private readonly ILoadServices _load;
        private readonly UserManager<AspNetUsers> _userManager;
        private readonly SignInManager<AspNetUsers> _signInManager;
        private readonly RoleManager<AspNetRoles> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailServices _emailServices;
        private readonly IBookingSmsServices _bookingSmsServices;
        private readonly ILogger<UserServices> _logger;



        public UserServices(UserManager<AspNetUsers> userManager,
            SignInManager<AspNetUsers> signInManager,
            RoleManager<AspNetRoles> roleManager,
            ILoadServices load,
            ApplicationDbContext context,
            IConfiguration configuration,
            IEmailServices emailServices,
            IBookingSmsServices bookingSmsServices,
            ILogger<UserServices> logger)
        {
            _load = load;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
            _emailServices = emailServices;
            _bookingSmsServices = bookingSmsServices;
            _logger = logger;
        }
        public async Task<IActionResult> CreateUserAsync(SignUpDto form)
        {
            try
            {
                // تحقق إذا كان المستخدم موجود مسبقًا
                var existingUser = await _userManager.FindByNameAsync(form.PhoneNumber.ToLower());
                if (existingUser != null)
                {
                    return new ConflictObjectResult(new ResponseDto<string>
                    {
                        Status = "Error",
                        Code = 409,
                        Message = "أسم المستخدم مضاف مسبقاً",
                        Data = null
                    });
                }

                // إنشاء كائن المستخدم الجديد
                var user = new AspNetUsers
                {
                    Name = form.Name,
                    UserName = form.PhoneNumber,
                    NormalizedUserName = form.PhoneNumber.ToUpper(),
                    PhoneNumber = form.PhoneNumber,
                    Email = form.Email,
                    ImageName = "default.png"
                };

                // إنشاء المستخدم
                var result = await _userManager.CreateAsync(user, form.Password);
                if (result.Succeeded)
                {
                    // تعيين الدور "NormalUser" للمستخدم
                    var role = await _roleManager.FindByNameAsync("NormalUser");
                    if (role != null)
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, role.Name);
                        if (roleResult.Succeeded)
                        {
                            return new OkObjectResult(new ResponseDto<object>
                            {
                                Status = "Success",
                                Code = 200,
                                Message = "تمت اضافة المستخدم بنجاح",
                                Data = new { userId = user.Id }
                            });
                        }
                        else
                        {
                            await _userManager.DeleteAsync(user);
                            return new BadRequestObjectResult(new ResponseDto<string>
                            {
                                Status = "Error",
                                Code = 400,
                                Message = "لم يتم اضافة المستخدم يرجى المحاولة لاحقاً!"
                            });
                        }
                    }
                }

                // في حالة فشل إنشاء المستخدم أو فشل تعيين الدور
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "لم يتم اضافة المستخدم يرجى المحاولة لاحقاً!"
                });
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database update exception: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");

                return new ObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "لم يتم الاضافة يرجى المحاولة لاحقاً"
                })
                {
                    StatusCode = 500
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected exception occurred: {ex.Message}");

                return new ObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "لم يتم الاضافة يرجى المحاولة لاحقاً"
                })
                {
                    StatusCode = 500
                };
            }
        }
        public async Task<IActionResult> LoginAsync(SignInDto form)
        {
            try
            {
                // البحث عن المستخدم بناءً على رقم الهاتف (اسم المستخدم)
                var user = await _userManager.FindByNameAsync(form.PhoneNumber);
                if (user == null)
                {
                    return new UnauthorizedObjectResult(new ResponseDto<string> { Status = "Error", Code = 401, Message = "أسم المستخدم او كلمة المرور خاطئة!" });

                }

                // تحقق من قفل الحساب إذا لم يكن SuperAdmin
                if (form.PhoneNumber != "superadmin" && await _userManager.GetAccessFailedCountAsync(user) >= 5)
                {
                    user.IsLocked = true;
                    await _userManager.UpdateAsync(user);
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddMinutes(30));

                    return new UnauthorizedObjectResult(new ResponseDto<string> { Status = "Error", Code = 401, Message = "تم اغلاق الحساب مؤقتا!" });

                }

                // تحقق من صحة كلمة المرور
                var isPassValid = await _userManager.CheckPasswordAsync(user, form.Password);
                if (!isPassValid)
                {
                    await _userManager.AccessFailedAsync(user);

                    var isLockedOut = await _userManager.IsLockedOutAsync(user);

                    return new UnauthorizedObjectResult(new ResponseDto<string> { Status = "Error", Code = 401, Message = "الحساب مغلق مؤقتاً!" });

                }

                // تحقق من حالة الحساب
                if (user.IsLocked)
                {
                    return new UnauthorizedObjectResult(new ResponseDto<string> { Status = "Error", Code = 401, Message = "الحساب مغلق يرجى الاتصال بادارة النظام!" });

                }
                if (user.IsDeleted)
                {
                    return new UnauthorizedObjectResult(new ResponseDto<string> { Status = "Error", Code = 401, Message = "الحساب مغلق يرجى الاتصال بادارة النظام!" });
                }

                // إعادة تعيين عداد المحاولات الفاشلة وإنهاء القفل إن وجد
                await _userManager.ResetAccessFailedCountAsync(user);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MinValue);

                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = BuildUserClaims(user, userRoles);

                // توليد التوكن
                var token = GetToken(authClaims);
                var refreshToken = CreateRefreshToken(user.Id);
                _context.RefreshTokens.Add(refreshToken.Entity);

                // تحديث وقت تسجيل الدخول الأخير
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                await _context.SaveChangesAsync();

                return new OkObjectResult(new ResponseDto<AuthTokenDto>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "تم تسجيل الدخول بنجاح",
                    Data = BuildAuthTokenDto(token, refreshToken.RawToken, refreshToken.Entity.ExpiresAt)
                });

            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update exception while signing in user {PhoneNumber}", form.PhoneNumber);

                return new ObjectResult(new ResponseDto<string> { Status = "Error", Code = 500, Message = "حاول مرة اخرى!" })
                {
                    StatusCode = 500
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception while signing in user {PhoneNumber}", form.PhoneNumber);

                return new ObjectResult(new ResponseDto<string> { Status = "Error", Code = 500, Message = "حاول مرة اخرى!" })
                {
                    StatusCode = 500
                };
            }
        }
        public async Task<IActionResult> RefreshTokenAsync(RefreshTokenDto form)
        {
            var refreshTokenValue = form.RefreshToken?.Trim();
            if (string.IsNullOrWhiteSpace(refreshTokenValue))
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "رمز تحديث الجلسة مطلوب."
                });
            }

            var refreshTokenHash = HashRefreshToken(refreshTokenValue);
            var storedToken = await _context.RefreshTokens
                .Include(token => token.User)
                .FirstOrDefaultAsync(token => token.TokenHash == refreshTokenHash && !token.IsDeleted);

            if (storedToken == null || storedToken.IsRevoked || storedToken.IsExpired || storedToken.User.IsDeleted || storedToken.User.IsLocked)
            {
                return new UnauthorizedObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 401,
                    Message = "انتهت الجلسة، يرجى تسجيل الدخول مرة أخرى."
                });
            }

            var userRoles = await _userManager.GetRolesAsync(storedToken.User);
            var authClaims = BuildUserClaims(storedToken.User, userRoles);
            var accessToken = GetToken(authClaims);
            var replacement = CreateRefreshToken(storedToken.UserId);
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.ReplacedByTokenHash = replacement.Entity.TokenHash;
            storedToken.ModifiedAt = DateTime.UtcNow;
            _context.RefreshTokens.Add(replacement.Entity);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<AuthTokenDto>
            {
                Status = "Success",
                Code = 200,
                Message = "تم تحديث الجلسة بنجاح.",
                Data = BuildAuthTokenDto(accessToken, replacement.RawToken, replacement.Entity.ExpiresAt)
            });
        }
        public async Task<IActionResult> RevokeRefreshTokenAsync(RefreshTokenDto form)
        {
            var refreshTokenValue = form.RefreshToken?.Trim();
            if (string.IsNullOrWhiteSpace(refreshTokenValue))
            {
                return new OkObjectResult(new ResponseDto<string>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "تم تسجيل الخروج."
                });
            }

            var refreshTokenHash = HashRefreshToken(refreshTokenValue);
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(token => token.TokenHash == refreshTokenHash && !token.IsDeleted);
            if (storedToken != null && !storedToken.IsRevoked)
            {
                storedToken.RevokedAt = DateTime.UtcNow;
                storedToken.ModifiedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return new OkObjectResult(new ResponseDto<string>
            {
                Status = "Success",
                Code = 200,
                Message = "تم تسجيل الخروج."
            });
        }
        public async Task<IActionResult> GetMyProfileAsync()
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return new UnauthorizedObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 401,
                    Message = "يرجى تسجيل الدخول."
                });
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "المستخدم غير موجود."
                });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault();
            var role = !string.IsNullOrWhiteSpace(roleName)
                ? await _roleManager.FindByNameAsync(roleName)
                : null;

            return new OkObjectResult(new ResponseDto<UserGetDto>
            {
                Status = "Success",
                Code = 200,
                Message = "تم جلب الملف الشخصي بنجاح.",
                Data = new UserGetDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    UserName = user.UserName,
                    ImageName = user.ImageName,
                    IsLocked = user.IsLocked,
                    IsFirstLogin = user.IsFirstLogin,
                    LastLoginDate = user.LastLoginDate,
                    IsDeleted = user.IsDeleted,
                    DeletedAt = user.DeletedAt,
                    RoleName = roleName,
                    RoleId = role?.Id
                }
            });
        }
        public async Task<IActionResult> UpdateMyProfileAsync(UserUpdateDto form)
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return new UnauthorizedObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 401,
                    Message = "يرجى تسجيل الدخول."
                });
            }

            return await UpdateUserAsync(userId.ToString(), form);
        }
        public async Task<IActionResult> UpdateUserAsync(string id, UserUpdateDto form)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.IsDeleted)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "المستخدم غير موجود",
                    Data = null
                });
            }

            if (string.Equals(user.UserName, "superadmin", StringComparison.OrdinalIgnoreCase))
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "لا يمكن تعديل حساب مدير النظام الأساسي.",
                    Data = null
                });
            }

            var phone = form.PhoneNumber.Trim();
            var email = form.Email.Trim();

            var phoneOwner = await _userManager.FindByNameAsync(phone);
            if (phoneOwner != null && phoneOwner.Id != user.Id)
            {
                return new ConflictObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 409,
                    Message = "رقم الهاتف مستخدم من حساب آخر.",
                    Data = null
                });
            }

            var emailOwner = await _userManager.FindByEmailAsync(email);
            if (emailOwner != null && emailOwner.Id != user.Id)
            {
                return new ConflictObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 409,
                    Message = "البريد الإلكتروني مستخدم من حساب آخر.",
                    Data = null
                });
            }

            var phoneChanged = !string.Equals(user.PhoneNumber, phone, StringComparison.OrdinalIgnoreCase);
            var emailChanged = !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase);

            user.Name = form.Name.Trim();
            user.PhoneNumber = phone;
            user.UserName = phone;
            user.NormalizedUserName = phone.ToUpperInvariant();
            user.Email = email;
            user.NormalizedEmail = email.ToUpperInvariant();

            if (phoneChanged)
            {
                user.PhoneNumberConfirmed = false;
            }

            if (emailChanged)
            {
                user.EmailConfirmed = false;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = $"فشل تعديل المستخدم: {string.Join(" | ", result.Errors.Select(e => e.Description))}",
                    Data = null
                });
            }

            return new OkObjectResult(new ResponseDto<string>
            {
                Status = "Success",
                Code = 200,
                Message = "تم تعديل بيانات المستخدم بنجاح",
                Data = null
            });
        }
        public async Task<IActionResult> SoftDeleteUserAsync(string id)
        {
            try
            {
                // البحث عن المستخدم
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return new NotFoundObjectResult(new ResponseDto<string>
                    {
                        Status = "Error",
                        Code = 404,
                        Message = "المستخدم غير موجود",
                        Data = null
                    });
                }

                // التحقق إذا كان محذوف مسبقًا
                if (user.IsDeleted)
                {
                    return new BadRequestObjectResult(new ResponseDto<string>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "المستخدم محذوف مسبقًا",
                        Data = null
                    });
                }

                // تفعيل Soft Delete
                user.IsDeleted = true;
                user.IsLocked = true; // اختياري: قفل الحساب إذا كان لا يزال بإمكانه تسجيل الدخول

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return new OkObjectResult(new ResponseDto<string>
                    {
                        Status = "Success",
                        Code = 200,
                        Message = "تم تعطيل حساب المستخدم بنجاح",
                        Data = null
                    });
                }

                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "حدث خطأ أثناء تعطيل الحساب",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");

                return new ObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "حدث خطأ غير متوقع أثناء تعطيل الحساب"
                });
            }
        }
        public async Task<IActionResult> ToggleUserLockStatusAsync(string id)
        {
            try
            {
                // البحث عن المستخدم
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return new NotFoundObjectResult(new ResponseDto<string>
                    {
                        Status = "Error",
                        Code = 404,
                        Message = "المستخدم غير موجود",
                        Data = null
                    });
                }

                // التبديل بين القفل والفتح
                if (user.IsLocked)
                {
                    // فتح الحساب
                    user.IsLocked = false;
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MinValue);

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return new OkObjectResult(new ResponseDto<string>
                        {
                            Status = "Success",
                            Code = 200,
                            Message = "تم فتح الحساب بنجاح",
                            Data = null
                        });
                    }
                }
                else
                {
                    // قفل الحساب
                    user.IsLocked = true;
                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddYears(100)); // قفل دائم أو مؤقت حسب ما تريد

                    var result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return new OkObjectResult(new ResponseDto<string>
                        {
                            Status = "Success",
                            Code = 200,
                            Message = "تم قفل الحساب بنجاح",
                            Data = null
                        });
                    }
                }

                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "حدث خطأ أثناء تحديث حالة الحساب",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");

                return new ObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "حدث خطأ غير متوقع أثناء تحديث حالة الحساب"
                });
            }
        }
        public async Task<IActionResult> GetPaginatedUsersAsync(Guid userGuid, string? search, int page = 1, int pageSize = 10)
        {
            var query = _userManager.Users
                .Where(u => !u.IsDeleted);

            if (userGuid != Guid.Empty)
            {
                query = query.Where(u => u.Id == userGuid);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u =>
                    (u.Name != null && u.Name.Contains(term)) ||
                    (u.UserName != null && u.UserName.Contains(term)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(term)) ||
                    (u.Email != null && u.Email.Contains(term)));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userDtos = new List<UserGetDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var roleName = roles.FirstOrDefault();
                var role = !string.IsNullOrEmpty(roleName) ? await _roleManager.FindByNameAsync(roleName) : null;

                userDtos.Add(new UserGetDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    UserName = user.UserName,
                    ImageName = user.ImageName,
                    IsLocked = user.IsLocked,
                    IsFirstLogin = user.IsFirstLogin,
                    LastLoginDate = user.LastLoginDate,
                    IsDeleted = user.IsDeleted,
                    DeletedAt = user.DeletedAt,
                    RoleName = roleName,
                    RoleId = role?.Id
                });
            }

            var pagedResult = new PaginationDto.PageResult<UserGetDto>(userDtos, totalItems, totalPages, page, pageSize);

            return new OkObjectResult(new ResponseDto<PaginationDto.PageResult<UserGetDto>>
            {
                Status = "Success",
                Code = 200,
                Message = "تم استرجاع المستخدمين مع الترقيم",
                Data = pagedResult
            });
        }
        public async Task<IActionResult> UploadImgAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return new BadRequestObjectResult(new ResponseDto<string>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "يرجى اختيار صورة صالحة."
                    });
                }

                const long maxFileSize = 5 * 1024 * 1024;
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".jpg", ".jpeg", ".png", ".webp"
                };
                if (file.Length > maxFileSize || !allowedExtensions.Contains(extension))
                {
                    return new BadRequestObjectResult(new ResponseDto<string>
                    {
                        Status = "Error",
                        Code = 400,
                        Message = "الصورة يجب أن تكون بصيغة JPG أو PNG أو WEBP وبحجم لا يتجاوز 5MB."
                    });
                }

                var user = _context.AspNetUsers.Where(d => d.Id == _load.GetCurrentUserId()).FirstOrDefault();

                if (user == null)
                {
                    var responseNotFound = new ResponseDto<string>
                    {
                        Status = "Error",
                        Code = 404,
                        Message = "المستخدم غير موجود",
                        Data = null
                    };
                    return new NotFoundObjectResult(responseNotFound)
                    {
                        StatusCode = 404
                    };
                }

                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UserImgProfile");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var fileName = Guid.NewGuid().ToString() + extension;

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UserImgProfile", fileName);
                var oldImagePath = user.ImageName == "default.png" || string.IsNullOrWhiteSpace(user.ImageName)
                    ? null
                    : Path.Combine(folderPath, Path.GetFileName(user.ImageName));

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                user.ImageName = fileName;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    if (oldImagePath != null && File.Exists(oldImagePath))
                    {
                        File.Delete(oldImagePath);
                    }

                    var successResponse = new ResponseDto<string>
                    {
                        Code = 200,
                        Message = "تم رفع الصورة بنجاح",
                        Data = null,
                        Status = "Success"
                    };
                    return new OkObjectResult(successResponse);
                }

                File.Delete(filePath);
                var bdResponse = new ResponseDto<string>
                {
                    Code = 400,
                    Message = "لم يتم رفع الصورة يرجى المحاولة لاحقا",
                    Data = null,
                    Status = "Error"
                };

                return new BadRequestObjectResult(bdResponse);
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database update exception: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");

                var errorResponse = new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = ex.Message,
                };

                return new ObjectResult(errorResponse)
                {
                    StatusCode = 500
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected exception occurred: {ex.Message}");

                var errorResponse = new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = ex.Message
                };

                return new ObjectResult(errorResponse)
                {
                    StatusCode = 500
                };
            }
        }
        public async Task<IActionResult> SendEmailConfirmationAsync(string identifier)
        {
            var user = await FindUserAsync(identifier);
            if (user == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "المستخدم غير موجود."
                });
            }

            var clientBaseUrl = _configuration["ClientApp:BaseUrl"] ?? _configuration["JWT:ValidAudience"];
            if (string.IsNullOrWhiteSpace(clientBaseUrl))
            {
                return new ObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "تعذر إنشاء رابط تأكيد البريد الإلكتروني."
                })
                {
                    StatusCode = 500
                };
            }

            string url_front = "http://localhost:5173";

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            // confirmationLink = $"{clientBaseUrl.TrimEnd('/')}/email-confirm?userId={user.Id}&token={WebUtility.UrlEncode(token)}";
            var confirmationLink = $"{url_front}/email-confirm?userId={user.Id}&token={WebUtility.UrlEncode(token)}";

            await _emailServices.SendAsync(user.Email, "تأكيد البريد الإلكتروني", _load.SandEmailHTMLTemplate(confirmationLink));

            return new OkObjectResult(new ResponseDto<string>
            {
                Status = "Success",
                Code = 200,
                Message = "تم إرسال رابط التفعيل إلى بريدك الإلكتروني."
            });
        }
        public async Task<IActionResult> ConfirmEmailAsync(Guid userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "المستخدم غير موجود"
                });
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return new OkObjectResult(new ResponseDto<string>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "تم تأكيد البريد الإلكتروني بنجاح"
                });
            }

            return new BadRequestObjectResult(new ResponseDto<string>
            {
                Status = "Error",
                Code = 400,
                Message = "فشل تأكيد البريد الإلكتروني، الرابط غير صالح أو منتهي"
            });
        }
        public async Task<IActionResult> SendPhoneConfirmationAsync()
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return new UnauthorizedObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 401,
                    Message = "يرجى تسجيل الدخول."
                });
            }

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null || user.IsDeleted)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "المستخدم غير موجود."
                });
            }

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "لا يوجد رقم هاتف مرتبط بالحساب."
                });
            }

            if (user.PhoneNumberConfirmed)
            {
                return new OkObjectResult(new ResponseDto<string>
                {
                    Status = "Success",
                    Code = 200,
                    Message = "رقم الهاتف مؤكد مسبقاً."
                });
            }

            var oldRequests = await _context.UserPhoneOtpRequests
                .Where(request => request.UserId == user.Id && !request.IsUsed)
                .ToListAsync();

            foreach (var request in oldRequests)
            {
                request.IsUsed = true;
                request.ModifiedAt = DateTime.UtcNow;
                request.ModifierId = user.Id;
            }

            var otpCode = GenerateNumericOtp(6);
            Console.WriteLine("//////////////////////////////");
            Console.WriteLine(otpCode);
            Console.WriteLine("//////////////////////////////");
            var codeSalt = GenerateOtpSalt();
            var now = DateTime.UtcNow;
            _context.UserPhoneOtpRequests.Add(new UserPhoneOtpRequest
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                CodeHash = HashOtpCode(otpCode, codeSalt),
                CodeSalt = codeSalt,
                SentAt = now,
                ExpiresAt = now.AddMinutes(10),
                CreatorId = user.Id
            });

            await _context.SaveChangesAsync();
            await _bookingSmsServices.SendBookingOtpAsync(user.PhoneNumber, otpCode);

            return new OkObjectResult(new ResponseDto<string>
            {
                Status = "Success",
                Code = 200,
                Message = "تم إرسال رمز تأكيد الهاتف."
            });
        }

        public async Task<IActionResult> ConfirmPhoneAsync(ConfirmPhoneDto form)
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return new UnauthorizedObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 401,
                    Message = "يرجى تسجيل الدخول."
                });
            }

            var user = await _userManager.FindByIdAsync(userId.Value.ToString());
            if (user == null || user.IsDeleted)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "المستخدم غير موجود."
                });
            }

            var otpCode = form.OtpCode?.Trim();
            if (string.IsNullOrWhiteSpace(otpCode))
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "أدخل رمز التأكيد."
                });
            }

            var request = await _context.UserPhoneOtpRequests
                .Where(item =>
                    item.UserId == user.Id &&
                    item.PhoneNumber == user.PhoneNumber &&
                    !item.IsUsed)
                .OrderByDescending(item => item.SentAt)
                .FirstOrDefaultAsync();

            if (request == null || request.ExpiresAt < DateTime.UtcNow)
            {
                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "رمز التأكيد غير صالح أو منتهي."
                });
            }

            if (request.AttemptCount >= 5)
            {
                request.IsUsed = true;
                request.ModifiedAt = DateTime.UtcNow;
                request.ModifierId = user.Id;
                await _context.SaveChangesAsync();

                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "تم تجاوز عدد المحاولات. اطلب رمزاً جديداً."
                });
            }

            request.AttemptCount++;
            if (!string.Equals(request.CodeHash, HashOtpCode(otpCode, request.CodeSalt), StringComparison.OrdinalIgnoreCase))
            {
                request.ModifiedAt = DateTime.UtcNow;
                request.ModifierId = user.Id;
                await _context.SaveChangesAsync();

                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "رمز التأكيد غير صحيح."
                });
            }

            request.IsUsed = true;
            request.VerifiedAt = DateTime.UtcNow;
            request.ModifiedAt = DateTime.UtcNow;
            request.ModifierId = user.Id;
            user.PhoneNumberConfirmed = true;
            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<string>
            {
                Status = "Success",
                Code = 200,
                Message = "تم تأكيد رقم الهاتف بنجاح."
            });
        }
        public async Task<IActionResult> SendResetPasswordLinkAsync(string identifier)
        {
            var user = await FindUserAsync(identifier);
            if (user == null || user.IsDeleted)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "هذا البريد غير مسجل لدينا."
                });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var clientBaseUrl = _configuration["ClientApp:BaseUrl"] ?? _configuration["JWT:ValidAudience"];
            if (string.IsNullOrWhiteSpace(clientBaseUrl))
            {
                return new ObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 500,
                    Message = "تعذر إنشاء رابط إعادة تعيين كلمة المرور."
                })
                {
                    StatusCode = 500
                };
            }

            string url_front = "http://localhost:5173";


            //var resetLink = $"{clientBaseUrl.TrimEnd('/')}/password-reset?userId={user.Id}&token={WebUtility.UrlEncode(token)}";
            var resetLink = $"{url_front}/password-reset?userId={user.Id}&token={WebUtility.UrlEncode(token)}";



            await _emailServices.SendAsync(user.Email, "إعادة تعيين كلمة المرور", _load.ResetPasswordHTMLTemplate(resetLink));

            return new OkObjectResult(new ResponseDto<string>
            {
                Status = "Success",
                Code = 200,
                Message = "تم إرسال رابط إعادة تعيين كلمة المرور إلى بريدك الإلكتروني."
            });
        }
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordDto form)
        {
            var user = await _userManager.FindByIdAsync(form.UserId.ToString());
            if (user == null || user.IsDeleted)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "المستخدم غير موجود."
                });
            }

            var result = await _userManager.ResetPasswordAsync(user, form.Token, form.NewPassword);

            if (!result.Succeeded)
            {
                var isExpired = result.Errors.Any(e => e.Code == "InvalidToken");
                var message = isExpired
                    ? "انتهت صلاحية الرابط، الرجاء طلب رابط جديد."
                    : $"فشل في تغيير كلمة المرور: {string.Join(" | ", result.Errors.Select(e => e.Description))}";

                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = message
                });
            }

            return new OkObjectResult(new ResponseDto<string>
            {
                Status = "Success",
                Code = 200,
                Message = "تم تغيير كلمة المرور بنجاح."
            });
        }
        public async Task<IActionResult> ChangePasswordAsync(ChangePasswordDto form)
        {
            var userId = _load.GetCurrentUserId();
            if (userId == null || userId == Guid.Empty)
            {
                return new UnauthorizedObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 401,
                    Message = "يرجى تسجيل الدخول."
                });
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.IsDeleted)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "المستخدم غير موجود."
                });
            }

            var result = await _userManager.ChangePasswordAsync(user, form.CurrentPassword, form.NewPassword);
            if (!result.Succeeded)
            {
                var message = result.Errors.Any(error => error.Code == "PasswordMismatch")
                    ? "كلمة المرور الحالية غير صحيحة."
                    : $"فشل تغيير كلمة المرور: {string.Join(" | ", result.Errors.Select(error => error.Description))}";

                return new BadRequestObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 400,
                    Message = message
                });
            }

            return new OkObjectResult(new ResponseDto<string>
            {
                Status = "Success",
                Code = 200,
                Message = "تم تغيير كلمة المرور بنجاح."
            });
        }

        private async Task<AspNetUsers?> FindUserAsync(string identifier)
        {
            var value = identifier?.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (Guid.TryParse(value, out var userId))
            {
                return await _userManager.FindByIdAsync(userId.ToString());
            }

            return await _userManager.FindByEmailAsync(value)
                ?? await _userManager.FindByNameAsync(value);
        }
        private List<Claim> BuildUserClaims(AspNetUsers user, IEnumerable<string> userRoles)
        {
            var authClaims = new List<Claim>
            {
                new Claim("username", user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim("Role", role));
                authClaims.Add(new Claim(ClaimTypes.Role, role));
                var roleId = GetRoleIdByName(role);
                if (!string.IsNullOrWhiteSpace(roleId))
                {
                    authClaims.Add(new Claim("roleId", roleId));
                }
            }

            return authClaims;
        }
        private string GetRoleIdByName(string roleName)
        {
            var role = _context.Roles.FirstOrDefault(r => r.Name == roleName);

            if (role != null)
            {
                return role.Id.ToString();
            }

            return null;
        }
        private AuthTokenDto BuildAuthTokenDto(JwtSecurityToken accessToken, string refreshToken, DateTime refreshTokenExpiresAt)
        {
            return new AuthTokenDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(accessToken),
                TokenExpiresAt = accessToken.ValidTo,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            };
        }
        private (RefreshToken Entity, string RawToken) CreateRefreshToken(Guid userId)
        {
            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return (new RefreshToken
            {
                UserId = userId,
                TokenHash = HashRefreshToken(rawToken),
                ExpiresAt = DateTime.UtcNow.AddDays(14),
                CreatorId = userId
            }, rawToken);
        }
        private static string HashRefreshToken(string token)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }
        private static string GenerateNumericOtp(int length)
        {
            var safeLength = Math.Clamp(length, 4, 8);
            var minValue = (int)Math.Pow(10, safeLength - 1);
            var maxValue = (int)Math.Pow(10, safeLength);
            return RandomNumberGenerator.GetInt32(minValue, maxValue).ToString();
        }
        private static string GenerateOtpSalt()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        }
        private static string HashOtpCode(string code, string salt)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{salt}:{code}")));
        }
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken
                (
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.UtcNow.AddMinutes(15),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}
