using Clinic_Booking.Data;
using Clinic_Booking.IServices.ILoadServices;
using System.Security.Claims;

namespace Clinic_Booking.Services.LoadServices
{
    public class LoadServices : ILoadServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _accessor;
        public LoadServices(ApplicationDbContext context,IHttpContextAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }
        public Guid? GetCurrentUserId()
        {
            var userId = _accessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userGuid = Guid.TryParse(userId , out var guid) ? guid : Guid.Empty;
            var user = _context.AspNetUsers.Where(us => us.Id == userGuid && !us.IsDeleted).FirstOrDefault();
            if (user != null)
            {
                return userGuid;
            }
            return Guid.Empty;
        }
    }
}
