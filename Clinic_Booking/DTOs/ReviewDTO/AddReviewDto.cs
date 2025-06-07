namespace Clinic_Booking.DTOs.ReviewDTO
{
    public class AddReviewDto
    {
        public int DoctorId { get; set; }
        public int AppointmentId { get; set; }
        public int Rating { get; set; } // بين 1 و 5
        public string Comment { get; set; }
    }
}
