using Clinic_Booking.Data;
using Clinic_Booking.DTOs.DoctorSubscriptionDTO;
using Clinic_Booking.DTOs.UserDTO;
using Clinic_Booking.Entities.DoctorSubscription;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic_Booking.Services.DoctorSubscriptionServices
{
    public class DoctorSubscriptionServices
    {
        private readonly ApplicationDbContext _context;
        public DoctorSubscriptionServices(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> CreateSubscriptionAsync(DoctorSubscriptionAddDto form)
        {
            var doctor = await _context.Doctors.Where(d=>d.Id == form.DoctorId && !d.IsDeleted).FirstOrDefaultAsync();
            if(doctor == null)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "يرجى اختيار عيادة اخرى!",
                    Data = null
                });
            }
            var now = DateTime.UtcNow;

            bool hasActive = await _context.DoctorSubscriptions
                .AnyAsync(ds => ds.DoctorId == form.DoctorId && ds.StartDate <= now && ds.EndDate >= now);

            if (hasActive)
            {
                return new BadRequestObjectResult(new ResponseDto<object>
                {
                    Status = "Error",
                    Code = 400,
                    Message = "الدكتور لديه اشتراك نشط بالفعل!",
                    Data = null
                });
            }

            var subscription = new DoctorSubscription
            {
                DoctorId = form.DoctorId,
                PackageId = form.PackageId,
                StartDate = now,
                EndDate = now.AddMonths(1)
            };

            _context.DoctorSubscriptions.Add(subscription);

            doctor.SubscriptionRank  = doctor.SubscriptionRank + 1;

            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseDto<object>
            {
                Status = "Success",
                Code = 200,
                Message = "تم إنشاء الاشتراك بنجاح.",
                Data = subscription
            });
        }
    }
}
