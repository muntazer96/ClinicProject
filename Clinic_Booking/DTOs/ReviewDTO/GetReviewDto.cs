using Clinic_Booking.Enums;

namespace Clinic_Booking.DTOs.ReviewDTO
{
    public class GetReviewDto
    {
        public int Id { get; set; }
        public GetUserReview User { get; set; }
        public GetDoctorReview Doctor { get; set; } // معرف الدكتور الذي تم تقييمه

        public int Rating { get; set; } // التقييم من 1 إلى 5
        public string Comment { get; set; } // تعليق المستخدم
        public int? AppoinmentId { get; set; }
        public GetAppointmentReview? Appointment { get; set; }
    }
    public class GetDoctorReview
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
    }
    public class GetUserReview
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NormalizedName { get; set; }
    }
    public class GetAppointmentReview
    {
        public int Id { get; set; }
        public AppointmentStatus Status { get; set; }
    }

    public class GetDoctorReviewsDto
    {
        public int DoctorId { get; set; }
        public bool IsEnabled { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<GetReviewDto> Reviews { get; set; } = [];
    }
}
