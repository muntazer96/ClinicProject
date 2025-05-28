using System.Net.NetworkInformation;

namespace Clinic_Booking.IServices.ILoadServices
{
    public interface ILoadServices
    {
        Guid? GetCurrentUserId();
        string SandEmailHTMLTemplate(string confirmationLink);
        string ResetPasswordHTMLTemplate(string resetLink);
    }
}
