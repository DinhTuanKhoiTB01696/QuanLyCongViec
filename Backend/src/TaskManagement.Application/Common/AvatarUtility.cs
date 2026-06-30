using System;
using System.Text.RegularExpressions;

namespace TaskManagement.Application.Common
{
    public static class AvatarUtility
    {
        private static readonly string[] AvatarColors = new[]
        {
            "#579dff", "#c97cf4", "#00b8d9", "#22a06b", "#f5cd47", "#e2483d"
        };

        public static string GetDeterministicColor(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                return "#0052CC"; // Fallback blue
            }

            int hash = 0;
            foreach (char c in identifier)
            {
                hash = c + ((hash << 5) - hash);
            }

            return AvatarColors[Math.Abs(hash) % AvatarColors.Length];
        }

        public static string GetInitials(string fullName, string email)
        {
            string source = !string.IsNullOrWhiteSpace(fullName) ? fullName : email;
            if (string.IsNullOrWhiteSpace(source))
            {
                return "U";
            }

            // If it's an email, just use the first letter of the prefix
            if (!string.IsNullOrWhiteSpace(email) && source == email)
            {
                var match = Regex.Match(email.Split('@')[0], @"\p{L}");
                return match.Success ? match.Value.ToUpper() : "U";
            }

            // For full names, try to extract first valid letter of up to first 2 words
            var words = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string initials = "";
            int count = 0;

            foreach (var word in words)
            {
                var match = Regex.Match(word, @"\p{L}");
                if (match.Success)
                {
                    initials += match.Value.ToUpper();
                    count++;
                }

                if (count >= 2) break;
            }

            return string.IsNullOrEmpty(initials) ? "U" : initials;
        }
    }
}
