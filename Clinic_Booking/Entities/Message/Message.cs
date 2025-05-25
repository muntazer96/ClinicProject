using Clinic_Booking.Entities.Shared;
using Clinic_Booking.Enums;

namespace Clinic_Booking.Entities.Message
{
    public class Message : BaseEntity<int>
    {
        public Guid SenderId { get; set; } // معرف المرسل (مستخدم)
        public User.AspNetUsers Sender { get; set; }

        public int ReceiverId { get; set; } // معرف المستقبل (دكتور)
        public Doctor.Doctor Receiver { get; set; }

        public string Content { get; set; } // محتوى الرسالة
        public DateTime SentAt { get; set; } // وقت الإرسال

        public MessageType Type { get; set; } // نوع الرسالة (استفسار، شكوى...)
    }
}
