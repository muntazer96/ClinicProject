using Clinic_Booking.IServices.ILoadServices;
using System.Security.Claims;

namespace Clinic_Booking.Services.LoadServices
{
    public class LoadServices : ILoadServices
    {
        private readonly IHttpContextAccessor _accessor;

        public LoadServices(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public Guid? GetCurrentUserId()
        {
            var userId = _accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userId, out var userGuid) ? userGuid : null;
        }
    }
}
