using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.IServices.ITelegramAlertService;

namespace Clinic_Booking.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly ITelegramAlertService _telegramAlert;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            ITelegramAlertService telegramAlert)
        {
            _next = next;
            _logger = logger;
            _telegramAlert = telegramAlert;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);

                var userId = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                await _telegramAlert.SendBackendExceptionAlertAsync(
                    httpContext.Request.Path,
                    httpContext.Request.Method,
                    userId,
                    ex.Message);

                await HandleExceptionAsync(httpContext);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new ResponseDto<string>
            {
                Status = "Error",
                Code = 500,
                Message = "حدث خطأ غير متوقع في الخادم.",
                Data = null
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
