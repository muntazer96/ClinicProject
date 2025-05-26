using Clinic_Booking.Data;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.Role;
using Clinic_Booking.Entities.User;
using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.IUserServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
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


        public UserServices(UserManager<AspNetUsers> userManager, SignInManager<AspNetUsers> signInManager, RoleManager<AspNetRoles> roleManager, ILoadServices load, ApplicationDbContext context, IConfiguration configuration)
        {
            _load = load;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
        }
        public async Task<IActionResult> CreateUserAsync(SignUpDto form)
        {
            try
            {
                var user = new AspNetUsers
                {
                    Name = form.Name,
                    UserName = form.PhoneNumber,
                    ImageName = "default.png",
                    NormalizedUserName = form.PhoneNumber.ToUpper(),
                    PhoneNumber = form.PhoneNumber,
                };

                var userisexist = await _userManager.FindByNameAsync(form.PhoneNumber.ToLower());
                if (userisexist != null)
                {
                    return new ConflictObjectResult(new ResponseDto<string> { Status = "Error", Code = 409, Message = "أسم المستخدم مضاف مسبقاً", Data = null });
                }

                var result = await _userManager.CreateAsync(user, form.Password);
                if (result.Succeeded)
                {
                    var role = await _roleManager.FindByNameAsync("NormalUser");
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(user, role.Name);
                        return new OkObjectResult(new ResponseDto<string> { Status = "Success", Code = 200, Message = "تمت اضافة المستخدم بنجاح", Data = null });
                    }
                }

                return new BadRequestObjectResult(new ResponseDto<string> { Status = "Error", Code = 400, Message = "لم يتم اضافة المستخدم يرجى المحاولة لاحقاً!" });
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database update exception: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");

                return new ObjectResult(new ResponseDto<string> { Status = "Error", Code = 500, Message = "لم يتم الاضافة يرجى المحاولة لاحقاً" })
                {
                    StatusCode = 500
                };
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions
                Console.WriteLine($"An unexpected exception occurred: {ex.Message}");

                return new ObjectResult(new ResponseDto<string> { Status = "Error", Code = 500, Message = "لم يتم الاضافة يرجى المحاولة لاحقاً" })
                {
                    StatusCode = 500
                };
            }
        }
        public async Task<IActionResult> LoginAsync(SignInDto form)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(form.PhoneNumber);
                if (user == null)
                {
                    return new UnauthorizedObjectResult(new ResponseDto<string> { Status = "Error", Code = 401, Message = "أسم المستخدم او كلمة المرور خاطئة!" });
                }

                if (form.PhoneNumber != "superadmin")
                {
                    if (await _userManager.GetAccessFailedCountAsync(user) >= 5)
                    {
                        user.IsLocked = true;
                        await _userManager.UpdateAsync(user);
                        await _userManager.SetLockoutEndDateAsync(user, DateTime.Now.AddMinutes(30));
                        return new UnauthorizedObjectResult(new ResponseDto<string> { Status = "Error", Code = 401, Message = "تم اغلاق الحساب مؤقتا!" });
                    }
                }

                var isPassValid = await _userManager.CheckPasswordAsync(user, form.Password);
                if (!isPassValid)
                {
                    await _userManager.AccessFailedAsync(user);
                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        return new UnauthorizedObjectResult(new ResponseDto<string> { Status = "Error", Code = 401, Message = "الحساب مغلق مؤقتاً!" });
                    }
                    else
                    {
                        return new UnauthorizedObjectResult(new ResponseDto<string> { Status = "Error", Code = 401, Message = "كلمة المرور خاطئة!" });
                    }
                }
                else
                {
                    if (user.IsLocked)
                    {
                        return new UnauthorizedObjectResult(new ResponseDto<string> { Status = "Error", Code = 401, Message = "الحساب مغلق يرجى الاتصال بادارة النظام!" });
                    }

                    await _userManager.ResetAccessFailedCountAsync(user);

                    await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MinValue);


                    var userRoles = await _userManager.GetRolesAsync(user);

                    var authClaims = new List<Claim>
                    {
                        new Claim("username", user.UserName),
                        new Claim("id", user.Id.ToString()),
                    };

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim("Role", userRole));
                        authClaims.Add(new Claim("roleId", GetRoleIdByName(userRole))); // Assuming GetRoleIdByName is a method that retrieves role ID by name
                    }

                    var token = GetToken(authClaims);

                    //await _userManager.SetAuthenticationTokenAsync(user, "MyAPIToken", "FirstToken", new JwtSecurityTokenHandler().WriteToken(token));

                    user.LastLoginDate = DateTime.Now;

                    await _userManager.UpdateAsync(user);

                    return new OkObjectResult(new ResponseDto<object> { Status = "Success", Code = 200, Message = "تم تسجيل الدخول بنجاح", Data = new { token = new JwtSecurityTokenHandler().WriteToken(token) } });
                }
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
                Console.WriteLine($"An unexpected exception occurred: {ex.Message}");

                return new ObjectResult(new ResponseDto<string> { Status = "Error", Code = 500, Message = "حاول مرة اخرى!" })
                {
                    StatusCode = 500
                };
            }
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
