using Clinic_Booking.Data;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.Role;
using Clinic_Booking.Entities.User;
using Clinic_Booking.IServices.IEmailServices;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.IUserServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
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



        public UserServices(UserManager<AspNetUsers> userManager,
            SignInManager<AspNetUsers> signInManager,
            RoleManager<AspNetRoles> roleManager,
            ILoadServices load,
            ApplicationDbContext context,
            IConfiguration configuration,
            IEmailServices emailServices)
        {
            _load = load;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
            _emailServices = emailServices;
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

                // إعداد صلاحيات المستخدم
                var userRoles = await _userManager.GetRolesAsync(user);
                var authClaims = new List<Claim>
                {
                    new Claim("username", user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };

                foreach (var role in userRoles)
                {
                    authClaims.Add(new Claim("Role", role));
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                    authClaims.Add(new Claim("roleId", GetRoleIdByName(role)));
                }

                // توليد التوكن
                var token = GetToken(authClaims);

                // تحديث وقت تسجيل الدخول الأخير
                user.LastLoginDate = DateTime.Now;
                await _userManager.UpdateAsync(user);

                return new OkObjectResult(new ResponseDto<object> { Status = "Success", Code = 200, Message = "تم تسجيل الدخول بنجاح", Data = new { token = new JwtSecurityTokenHandler().WriteToken(token) } });

            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database update exception: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");

                return new ObjectResult(new ResponseDto<string> { Status = "Error", Code = 500, Message = "حاول مرة اخرى!" })
                {
                    StatusCode = 500
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected exception: {ex.Message}");

                return new ObjectResult(new ResponseDto<string> { Status = "Error", Code = 500, Message = "حاول مرة اخرى!" })
                {
                    StatusCode = 500
                };
            }
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
                    Email = user.Email,
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

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{clientBaseUrl.TrimEnd('/')}/email-confirm?userId={user.Id}&token={WebUtility.UrlEncode(token)}";

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

            var resetLink = $"{clientBaseUrl.TrimEnd('/')}/password-reset?userId={user.Id}&token={WebUtility.UrlEncode(token)}";



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
        private string GetRoleIdByName(string roleName)
        {
            var role = _context.Roles.FirstOrDefault(r => r.Name == roleName);

            if (role != null)
            {
                return role.Id.ToString();
            }

            return null;
        }
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken
                (
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddDays(30),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
    }
}
