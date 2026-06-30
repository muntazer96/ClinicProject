using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Clinic_Booking.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this System.Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()?
                            .GetName() ?? enumValue.ToString();
        }
    }
}
