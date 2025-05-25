using Clinic_Booking.Entities.Shared;
using Clinic_Booking.Enums;

namespace Clinic_Booking.Entities.Notification
{
    public class Notification : BaseEntity<int>
    {
        public string Message { get; set; } // نص الإشعار
        public DateTime CreatedAt { get; set; } // وقت الإنشاء
        public NotificationStatus Status { get; set; } = NotificationStatus.Unread; // حالة الإشعار (مقروء، غير مقروء)

        public Guid? UserId { get; set; } // معرف المستخدم (إن وجد)
        public User.AspNetUsers User { get; set; }

        public int? DoctorId { get; set; } // معرف الدكتور (إن وجد)
        public Doctor.Doctor Doctor { get; set; }
    }
}
