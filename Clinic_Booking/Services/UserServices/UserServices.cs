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
using System;
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
        private readonly IHttpContextAccessor _httpContextAccessor;



        public UserServices(UserManager<AspNetUsers> userManager,
            SignInManager<AspNetUsers> signInManager,
            RoleManager<AspNetRoles> roleManager,
            ILoadServices load,
            ApplicationDbContext context,
            IConfiguration configuration,
            IEmailServices emailServices,
            IHttpContextAccessor httpContextAccessor)
        {
            _load = load;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
            _emailServices = emailServices;
            _httpContextAccessor = httpContextAccessor;
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
                            return new OkObjectResult(new ResponseDto<string>
                            {
                                Status = "Success",
                                Code = 200,
                                Message = "تمت اضافة المستخدم بنجاح",
                                Data = null
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
        public async Task<IActionResult> GetPaginatedUsersAsync(Guid UserGUID, int page = 1, int pageSize = 10)
        {
            var query = _userManager.Users
                .Where(u => !u.IsDeleted);

            if (UserGUID != Guid.Empty)
            {
                query = query.Where(u => u.Id == UserGUID);
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

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UserImgProfile", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                if (user.ImageName != "default.png" && File.Exists(folderPath))
                {
                    File.Delete(folderPath);
                }

                user.ImageName = fileName;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    var successResponse = new ResponseDto<string>
                    {
                        Code = 200,
                        Message = "تم رفع الصورة بنجاح",
                        Data = null,
                        Status = "Success"
                    };
                    return new OkObjectResult(successResponse);
                }

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
        public async Task<IActionResult> SendEmailConfirmationAsync(string guid)
        {
            var user = await _userManager.FindByIdAsync(guid);
            if (user == null)
            {
                return new NotFoundObjectResult(new ResponseDto<string>
                {
                    Status = "Error",
                    Code = 404,
                    Message = "المستخدم غير موجود."
                });
            }

            var request = _httpContextAccessor.HttpContext.Request;
            var scheme = request.Scheme;
            var host = request.Host.Value;

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"{scheme}://{host}/api/User/ConfirmEmail?userId={user.Id}&token={WebUtility.UrlEncode(token)}";

            // قالب البريد HTML
            var htmlBody = $@"
<!DOCTYPE html>
<html lang='ar' dir='rtl'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f7f7f7;
            margin: 0;
            padding: 0;
        }}
        .email-container {{
            max-width: 600px;
            margin: auto;
            background-color: #ffffff;
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
            direction: rtl;
        }}
        .header {{
            background-color: #2a9d8f;
            color: #ffffff;
            padding: 20px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
        }}
        .body {{
            padding: 30px;
            color: #333333;
            line-height: 1.8;
        }}
        .body p {{
            margin: 0 0 15px;
        }}
        .button {{
            display: inline-block;
            padding: 12px 25px;
            background-color: #2a9d8f;
            color: #ffffff;
            text-decoration: none;
            border-radius: 6px;
            margin-top: 20px;
        }}
        .footer {{
            padding: 15px;
            text-align: center;
            font-size: 13px;
            color: #999999;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <h1>تأكيد بريدك الإلكتروني</h1>
        </div>
        <div class='body'>
            <p>مرحباً بك في نظام حجز العيادات الإلكترونية،</p>
            <p>يرجى تأكيد بريدك الإلكتروني لتفعيل حسابك والوصول إلى خدمات النظام.</p>
            <p style='text-align: center;'>
                <a href='{confirmationLink}' class='button'>تأكيد البريد</a>
            </p>
            <p>إذا لم تقم بإنشاء حساب، يمكنك تجاهل هذه الرسالة.</p>
        </div>
        <div class='footer'>
            &copy; 2025 نظام حجز العيادات الإلكترونية. جميع الحقوق محفوظة.
        </div>
    </div>
</body>
</html>
";

            await _emailServices.SendAsync(user.Email, "تأكيد البريد الإلكتروني", htmlBody);

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
