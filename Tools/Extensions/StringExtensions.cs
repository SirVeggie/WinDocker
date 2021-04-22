using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Apprentice.Tools.Extensions {
    public static class StringExtensions {

        public static string CamelCaseToWords(this string s) {
            string res = Regex.Replace(s, @"([A-Z]+?(?=[A-Z][a-z])|[A-Z]+(?=([^A-Z]|$))|[0-9]+(?=([^0-9])|$))", " $1");
            return res.Remove(@"^\s+").ToUpperFirst();
        }

        public static string ToUpperFirst(this string s) {
            return s.First().ToString().ToUpper() + s.Substring(1);
        }

        public static string Truncate(this string s, int maxLength) {
            if (string.IsNullOrEmpty(s) || s.Length <= maxLength)
                return s;
            return s.Substring(0, maxLength);
        }

        public static string TruncateFancy(this string s, int maxLength) {
            if (string.IsNullOrEmpty(s) || s.Length <= Math.Max(3, maxLength))
                return s;
            return s.Substring(0, maxLength - 3) + "...";
        }

        public static string FromEnd(this string s, int length) {
            if (string.IsNullOrEmpty(s))
                return s;
            return s.Substring(s.Length - Math.Min(s.Length, length));
        }

        public static string RemoveEnd(this string s, int length) {
            if (string.IsNullOrEmpty(s))
                return s;
            return s.Remove(s.Length - Math.Min(s.Length, length));
        }

        public static string[] Split(this string s, params string[] separator) => s.Split(separator, StringSplitOptions.None);
        public static string[] Split(this string s, char separator, int count) => s.Split(new char[] { separator }, count);
        public static string[] Split(this string s, string separator, int count) => s.Split(new string[] { separator }, count, StringSplitOptions.None);
        /// <summary>Splits string according to given separators, excludes empty splits from the resulting array</summary>
        public static string[] SplitNoEmpty(this string s, params string[] separator) => s.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        public static string[] SplitNoEmpty(this string s, char separator, int count) => s.Split(new char[] { separator }, count, StringSplitOptions.RemoveEmptyEntries);
        public static string[] SplitNoEmpty(this string s, string separator, int count) => s.Split(new string[] { separator }, count, StringSplitOptions.RemoveEmptyEntries);

        #region regex
        /// <summary>Remove parts of a string by using Regex.</summary>
        public static string Remove(this string s, string regexPattern, RegexOptions options = RegexOptions.None) {
            return Regex.Replace(s, regexPattern, "", options);
        }

        /// <summary>Returns a new string with all occurrences replaced.</summary>
        public static string Replace(this string s, string regexPattern, string replacement, RegexOptions options) {
            return Regex.Replace(s, regexPattern, replacement, options);
        }

        public static Match Match(this string s, string regexPattern, RegexOptions options = RegexOptions.None) {
            return Regex.Match(s, regexPattern, options);
        }

        public static bool IsMatch(this string s, string regexPattern, RegexOptions options = RegexOptions.None) {
            return Regex.IsMatch(s, regexPattern, options);
        }
        #endregion
    }
}
