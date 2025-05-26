using Clinic_Booking.IServices.ILoadServices;
using Clinic_Booking.IServices.IUserServices;

namespace Clinic_Booking.Services.UserServices
{
    public class UserServices : IUserServices
    {
        private readonly ILoadServices _load;
        public UserServices(ILoadServices load)
        {
            _load = load;
        }
    }
}
