using Clinic_Booking.Data;
using Clinic_Booking.IServices.ILoadServices;
using System.Security.Claims;

namespace Clinic_Booking.Services.LoadServices
{
    public class LoadServices : ILoadServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _accessor;
        public LoadServices(ApplicationDbContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }
        public Guid? GetCurrentUserId()
        {
            var userId = _accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userId, out var userGuid))
            {
                return userGuid;
            }
            return null;
        }
        public string SandEmailHTMLTemplate(string confirmationLink)
        {
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
            return htmlBody;
        }
        public string ResetPasswordHTMLTemplate(string resetLink)
        {
            var htmlBody = $@"
        <html dir='rtl'>
        <body style='font-family:Arial,sans-serif; background-color:#f9f9f9; padding:20px;'>
            <div style='max-width:600px;margin:auto;background-color:#fff;padding:20px;border-radius:10px;box-shadow:0 2px 5px rgba(0,0,0,0.1);'>
                <h2 style='color:#2a9d8f;text-align:center;'>إعادة تعيين كلمة المرور</h2>
                <p>مرحباً،</p>
                <p>لقد طلبت إعادة تعيين كلمة المرور الخاصة بك. يمكنك ذلك من خلال النقر على الزر التالي:</p>
                <div style='text-align:center;margin:30px 0;'>
                    <a href='{resetLink}' style='background-color:#2a9d8f;color:#fff;padding:12px 24px;text-decoration:none;border-radius:5px;'>إعادة تعيين كلمة المرور</a>
                </div>
                <p>إذا لم تطلب هذه العملية، يمكنك تجاهل هذا البريد.</p>
                <hr />
                <p style='text-align:center;font-size:12px;color:#888;'>نظام حجز العيادات الإلكترونية</p>
            </div>
        </body>
        </html>";
            return htmlBody;
        }
    }
}
