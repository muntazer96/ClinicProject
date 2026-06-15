using System.Text.RegularExpressions;

namespace Clinic_Booking.Services.ProfanityFilterService
{
    public class ProfanityFilterServices
    {
        private static readonly string[] BlockedWords =
        {
            // Arabic profanity
            "كس", "شرموط", "قحبة", "عرص", "متناك", "منيوك", "خول", "عير",
            "زب", "طيز", "لبوة", "احا", "نيوك", "مقحوب", "مخنث", "كحبة",
            "انعل", "سحق", "لوطي", "لائطي", "لاط", "سحاق", "سحاقية",
            "خنيث", "بهيم", "منحرف", "قواد", "ديوث", "مومس", "عاهرة",
            "شرموطة", "قحب", "مقحوب", "منحوس",
            // Arabic evaluation/rating spam related
            "تقييم", "مراجعة", "نجمة", "نجوم", "تصويت", "صوت", "تصنيف",
            "تزوير", "وهمي", "حسابات وهمية", "تعليقات", "كومنت",
            // English profanity
            "fuck", "shit", "ass", "bitch", "bastard", "damn", "crap",
            "dick", "cock", "pussy", "whore", "slut", "cunt", "motherfucker",
            "asshole", "bullshit", "faggot", "nigger", "twat", "wanker",
            "prick", "douche", "bollocks", "arse", "bloody", "bugger",
            // English evaluation/rating spam related
            "rate", "rating", "review", "star", "stars", "vote", "voting",
            "spam", "fake", "bot", "scam", "fraud",
            // Common leetspeak / bypass attempts
            "fck", "fuk", "sh1t", "b1tch", "btch", "a55", "a$$"
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
