using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Blog.Helper
{
    public static class StringHelper
    {
        public static string BlogPostSlug(string? title)
        {
            string? output = RemoveAccents(title).ToLower();
            //Remove special characters
            output = Regex.Replace(output, @"[^A-Za-z0-9\s-]", "");
            //Remove all additional spaces
            output = Regex.Replace(output, @"\s+", " ");
            //Remove all spaces with the hyphen
            output = Regex.Replace(output, @"\s", "-");
            return output;

        }
        private static string RemoveAccents(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return title!;
            }
            // Convert for Unicode
            title = title.Normalize(NormalizationForm.FormD);
            // Format unicode/ascii
            char[] chars = title.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            // Convert and return the new title
            return new string(chars).Normalize(NormalizationForm.FormC);
        }
    }
}
