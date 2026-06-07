using System.Text.RegularExpressions;

namespace Clinic_Booking.Services.ProfanityFilterService
{
    public class ProfanityFilterServices
    {
        private static readonly string[] BlockedWords =
        {
        "كلمة1",
        "كلمة2",
        "كلمة3"
        };

        public static bool ContainsProfanity(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var normalizedText = Normalize(text);

            return BlockedWords.Any(word =>
                normalizedText.Contains(Normalize(word)));
        }

        private static string Normalize(string text)
        {
            text = text.Trim().ToLowerInvariant();

            // حذف المسافات والرموز حتى يمنع التحايل
            text = Regex.Replace(text, @"[\s\-_\.،,;:!؟?@#$%^&*()+=\[\]{}\\/|""']", "");

            // توحيد بعض الحروف العربية
            text = text.Replace("أ", "ا")
                       .Replace("إ", "ا")
                       .Replace("آ", "ا")
                       .Replace("ة", "ه")
                       .Replace("ى", "ي");

            return text;
        }
    }
}
