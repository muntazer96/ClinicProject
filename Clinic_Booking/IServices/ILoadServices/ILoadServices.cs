namespace Clinic_Booking.IServices.ILoadServices
{
    public interface ILoadServices
    {
        Guid? GetCurrentUserId();
        string? GetCurrentUsername();
    }
}
