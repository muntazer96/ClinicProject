using Clinic_Booking.DTOs.SharedDTO;
using Clinic_Booking.Entities.Specialization;
using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.DoctorDTO
{
    public class GetDoctorDto
    {
        public int Id { get; set; }
        public string Name { get; set; } // اسم الدكتور
        public string NormalizedName { get; set; } // اسم الدكتور
        public GetItemsDto Specialization { get; set; }
        public string Description { get; set; } // وصف الدكتور أو خبرته

        // عدد مرات الاشتراك في النظام (حسب الاشتراك)
        public int SubscriptionRank { get; set; }
        public IraqiProvince IraqiProvince { get; set; }
        public string IraqiProvinceName { get; set; }
        public string IraqiProvinceNormalizedName { get; set; }
        
        public string ImageName { get; set; }
        public DateOnly BirthDay { get; set; }
        public string PhoneNumber { get; set; }
        public string Location { get; set; }
        public bool IsPubliclyVisible { get; set; }
    }
}
