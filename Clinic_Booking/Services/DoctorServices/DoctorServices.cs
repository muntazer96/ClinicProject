using Clinic_Booking.Data;
using Clinic_Booking.IServices.IDoctorServices;

namespace Clinic_Booking.Services.DoctorServices
{
    public class DoctorServices : IDoctorServices
    {
        private readonly ApplicationDbContext _context;
        public DoctorServices(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
