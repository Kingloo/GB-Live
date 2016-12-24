using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GB_Live.Extensions
{
    public static class StringExtensions
    {
        public static IReadOnlyList<string> FindBetween(this string text, string beginning, string ending)
        {
            if (text == null) { throw new ArgumentNullException(nameof(text)); }
            
            if (String.IsNullOrEmpty(beginning))
            { throw new ArgumentException("beginning was NullOrEmpty", nameof(beginning)); }

            if (String.IsNullOrEmpty(ending))
            { throw new ArgumentException("ending was NullOrEmpty", nameof(ending)); }

            List<string> results = new List<string>();

            string pattern = string.Format(
                CultureInfo.CurrentCulture,
                "{0}({1}){2}",
                Regex.Escape(beginning),
                ".+?",
                Regex.Escape(ending));

            foreach (Match m in Regex.Matches(text, pattern))
            {
                results.Add(m.Groups[1].Value);
            }

            return results;
        }
    }
}
