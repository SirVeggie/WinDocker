using System;
using System.Collections.Generic;
using System.Linq;

namespace Apprentice.Tools.Extensions {
    public static class StringMatchingExtensions {
        public static int EditDistance(this string from, string to) => StringMatching.EditDistance(from, to);
        /// <param name="cReplace">Cost of replace operation. Default is 1.</param>
        /// <param name="cInsert">Cost of insert operation. Default is 1.</param>
        /// <param name="cDelete">Cost of delete operation. Default is 1.</param>
        public static int EditDistance(this string from, string to, int cReplace, int cInsert, int cDelete) => StringMatching.EditDistance(from, to, cReplace, cInsert, cDelete);
        /// <summary>Find the best approximate match for <paramref name="pattern"/> in <paramref name="text"/></summary>
        public static StrMatch Find(this string text, string pattern) => StringMatching.Find(text, pattern);
        /// <summary>Find the best approximate match for <paramref name="pattern"/> in <paramref name="text"/></summary>
        /// <param name="cReplace">Cost of replace operation. Default is 1.</param>
        /// <param name="cInsert">Cost of insert operation. Default is 1.</param>
        /// <param name="cDelete">Cost of delete operation. Default is 1.</param>
        public static StrMatch Find(this string text, string pattern, int cReplace, int cInsert, int cDelete) => StringMatching.Find(text, pattern, cReplace, cInsert, cDelete);
        /// <summary>Find the best approximate match for <paramref name="words"/> in <paramref name="text"/>. Also takes into account the <paramref name="words"/>'s distance from the beginning.</summary>
        /// <remarks>Uses case insensitive matching. Matches each word</remarks>
        public static StrMatch WordMatch(this string text, string words) => StringMatching.WordMatch(text, words);
        /// <summary>Find the best approximate match for <paramref name="words"/> in <paramref name="text"/>. Also takes into account the <paramref name="words"/>'s distance from the beginning.</summary>
        public static StrMatch WordMatch(this string text, params string[] words) => StringMatching.WordMatch(text, words);
        /// <summary>Find the best approximate match for <paramref name="words"/> in <paramref name="text"/>. Also takes into account the <paramref name="words"/>'s distance from the beginning.</summary>
        /// <param name="cReplace">Cost of replace operation. Default is 1.</param>
        /// <param name="cInsert">Cost of insert operation. Default is 1.</param>
        /// <param name="cDelete">Cost of delete operation. Default is 1.</param>
        public static StrMatch WordMatch(this string text, string words, int cReplace, int cInsert, int cDelete) => StringMatching.WordMatch(text, words, cReplace, cInsert, cDelete);
        /// <summary>Find the best approximate match for <paramref name="words"/> in <paramref name="text"/>. Also takes into account the <paramref name="words"/>'s distance from the beginning.</summary>
        /// <param name="cReplace">Cost of replace operation. Default is 1.</param>
        /// <param name="cInsert">Cost of insert operation. Default is 1.</param>
        /// <param name="cDelete">Cost of delete operation. Default is 1.</param>
        public static StrMatch WordMatch(this string text, string[] words, int cReplace, int cInsert, int cDelete) => StringMatching.WordMatch(text, words, cReplace, cInsert, cDelete);
    }
}

namespace Apprentice.Tools {

    public struct StrMatch {

        /// <summary>Match was found succesfully</summary>
        public bool Success { get; private set; }
        /// <summary>The entire matching substring</summary>
        public string Match { get; private set; }
        /// <summary>Calculated value of the match where smaller is better</summary>
        public int Distance { get; private set; }
        /// <summary>The starting point of the found substring</summary>
        public int Start { get; private set; }
        /// <summary>Length of the found substring</summary>
        public int Length { get; private set; }
        /// <summary>Length of the full text that was searched</summary>
        public int TextLength { get; private set; }
        /// <summary>Percentage of how good the match was</summary>
        public double Integrity { get; private set; }

        public StrMatch(string match, int distance, int start, int length, int textLength, double integrity) {
            Success = true;
            Match = match;
            Distance = distance;
            Start = start;
            Length = length;
            TextLength = textLength;
            Integrity = integrity;
        }

        public static StrMatch Fail() => new StrMatch {
            Success = false,
            Match = null,
            Distance = int.MaxValue,
            Start = int.MinValue,
            Length = int.MinValue,
            TextLength = int.MinValue,
            Integrity = 0
        };

        public override string ToString() => Success ? $"{{ StrMatch: Match: {Match}, Distance: {Distance}, Integrity: {Math.Round(Integrity, 2)} }}" : "{ StrMatch: Fail }";
    }

    public static class StringMatching {
        #region edit distance
        /// <summary>Calculate the edit distance between two strings</summary>
        public static int EditDistance(string text, string pattern, bool caseSensitive = true) => EditDistance(text, pattern, caseSensitive, 1, 1, 1);
        /// <summary>Calculate the edit distance between two strings. Specify custom costs for the operations.</summary>
        public static int EditDistance(string text, string pattern, int cReplace, int cInsert, int cDelete) => EditDistance(text, pattern, true, cReplace, cInsert, cDelete);
        /// <summary>Calculate the edit distance between two strings. Specify custom costs for the operations.</summary>
        public static int EditDistance(string text, string pattern, bool caseSensitive, int cReplace, int cInsert, int cDelete) {
            if (text == null || pattern == null) {
                throw new ArgumentException("Strings can't be null");
            }

            int[,] matrix = new int[text.Length + 1, pattern.Length + 1];

            for (int i = 0; i <= text.Length; i++)
                matrix[i, 0] = i * cDelete;
            for (int i = 1; i <= pattern.Length; i++)
                matrix[0, i] = i * cInsert;
            for (int y = 1; y <= pattern.Length; y++) {
                for (int x = 1; x <= text.Length; x++) {
                    bool isMatch = caseSensitive ? text[x - 1] == pattern[y - 1] : char.ToLower(text[x - 1]) == char.ToLower(pattern[y - 1]);
                    var replace = matrix[x - 1, y - 1] + (isMatch ? 0 : cReplace);
                    var insert = matrix[x, y - 1] + cInsert;
                    var delete = matrix[x - 1, y] + cDelete;
                    matrix[x, y] = Math.Min(Math.Min(insert, delete), replace);
                }
            }

            return matrix[text.Length, pattern.Length];
        }

        public static string BestMatch(string text, string[] patterns, Func<string, string, int> comparer) {
            int dist = int.MaxValue;
            string best = null;

            foreach (var p in patterns) {
                var d = comparer(text, p);
                if (d < dist) {
                    dist = d;
                    best = p;
                }
            }

            return best;
        }

        public static string BestMatch(string text, string[] patterns, bool caseSensitive = true) {
            int dist = int.MaxValue;
            string best = null;

            foreach (var p in patterns) {
                var d = EditDistance(text, p, caseSensitive);
                if (d < dist) {
                    dist = d;
                    best = p;
                }
            }

            return best;
        }
        #endregion

        #region substring matching
        /// <summary>Find the best matching substring from a string. Substring version of Edit Distance.</summary>
        public static StrMatch Find(string text, string pattern) => Find(text, pattern, 1, 1, 1);
        /// <summary>Find the best matching substring from a string. Substring version of Edit Distance.
        /// Specify custom costs for the operations.</summary>
        public static StrMatch Find(string text, string pattern, int cReplace, int cInsert, int cDelete) {
            if (text == null || pattern == null) {
                throw new ArgumentException("Strings can't be null");
            }

            int[,] matrix = new int[pattern.Length + 1, text.Length + 1];

            for (int i = 0; i <= pattern.Length; i++)
                matrix[i, 0] = i * cDelete;
            for (int i = 1; i <= text.Length; i++)
                matrix[0, i] = 0;
            for (int y = 1; y <= text.Length; y++) {
                for (int x = 1; x <= pattern.Length; x++) {
                    bool isMatch = pattern[x - 1] == text[y - 1];
                    var replace = matrix[x - 1, y - 1] + (isMatch ? 0 : cReplace);
                    var insert = matrix[x, y - 1] + cInsert;
                    var delete = matrix[x - 1, y] + cDelete;
                    matrix[x, y] = Math.Min(Math.Min(insert, delete), replace);
                }
            }

            var best = matrix[pattern.Length, 0];
            var start = 0;
            var end = 0;

            for (int i = 1; i <= text.Length; i++) {
                var value = matrix[pattern.Length, i];
                if (value < best) {
                    best = value;
                    end = i;
                }
            }

            if (best == pattern.Length) {
                return StrMatch.Fail();
            }

            int X = pattern.Length;
            int Y = end;

            while (X != 0) {
                var left = matrix[X - 1, Y];
                var corner = matrix[X - 1, Y - 1];
                var up = matrix[X, Y - 1];

                if (left <= corner && left <= up) {
                    X--;
                } else if (corner <= up) {
                    X--;
                    Y--;
                } else {
                    Y--;
                }

                start = Y;
            }

            start -= 1;
            end -= 1;
            var length = end - start + 1;

            return new StrMatch(text.Substring(start, length), best, start, length, text.Length, 1 - best / (double) pattern.Length);
        }
        #endregion

        #region word matching
        /// <summary>
        /// Calculate how well the given words match the string.
        /// Space functions as a word separator in the pattern.
        /// Uses case insensitive matching.
        /// </summary>
        public static StrMatch WordMatch(string text, string pattern) => WordMatch(text, pattern, 1, 1, 1);

        /// <summary>
        /// Calculate how well the given words match the string.
        /// Uses case insensitive matching.
        /// </summary>
        public static StrMatch WordMatch(string text, params string[] words) => WordMatch(text, words, 1, 1, 1);

        /// <summary>
        /// Calculate how well the given words match the string.
        /// Space functions as a word separator in the pattern.
        /// Uses case insensitive matching.
        /// Specify custom costs for the operations.
        /// </summary>
        public static StrMatch WordMatch(string text, string pattern, int cReplace, int cInsert, int cDelete) => WordMatch(text, pattern.Trim(' ').Split(' ').Where(w => w.Length > 0).ToArray(), cReplace, cInsert, cDelete);

        /// <summary>
        /// Calculate how well the given words match the string.
        /// Uses case insensitive matching.
        /// Specify custom costs for the operations.
        /// </summary>
        public static StrMatch WordMatch(string text, string[] words, int cReplace, int cInsert, int cDelete) {
            if (text == null || words == null || words.Length == 0)
                throw new ArgumentException("Arguments cannot be null or empty");
            string pattern = string.Join("", words);

            // Find word borders
            var slack = new HashSet<int>();
            var index = 0;

            foreach (var word in words) {
                index += word.Length;
                slack.Add(index);
            }

            // Start node tracker
            var startNode = new int[pattern.Length + 1, text.Length + 1];
            for (int y = 0; y < startNode.GetLength(1); y++) {
                startNode[0, y] = y;
            }

            // Calculate custom edit distance
            var matrix = new int[pattern.Length + 1, text.Length + 1];

            for (int x = 0; x < matrix.GetLength(0); x++)
                matrix[x, 0] = x * cDelete;
            for (int y = 1; y < matrix.GetLength(1); y++)
                matrix[0, y] = 0;
            for (int y = 1; y <= text.Length; y++) {
                for (int x = 1; x <= pattern.Length; x++) {
                    bool isMatch = char.ToLower(pattern[x - 1]) == char.ToLower(text[y - 1]);
                    int replace = matrix[x - 1, y - 1] + (isMatch ? 0 : cReplace);
                    int insert = matrix[x, y - 1] + (slack.Contains(x) ? 0 : cInsert);
                    int delete = matrix[x - 1, y] + cDelete;

                    if (replace <= insert && replace <= delete) {
                        matrix[x, y] = replace;
                        startNode[x, y] = startNode[x - 1, y - 1];
                    } else if (insert <= delete) {
                        matrix[x, y] = insert;
                        startNode[x, y] = startNode[x, y - 1];
                    } else {
                        matrix[x, y] = delete;
                        startNode[x, y] = startNode[x - 1, y];
                    }
                }
            }

            // Find best match distance and end position
            int best = matrix[pattern.Length, 0];
            int end = 0;

            for (int i = 1; i <= text.Length; i++) {
                int value = matrix[pattern.Length, i];

                if (value < best) {
                    best = value;
                    end = i;
                }
            }

            // Match failed
            if (best == pattern.Length) {
                return StrMatch.Fail();
            }

            // Final values
            int start = startNode[pattern.Length, end];
            int matchLength = end - start;
            int middleInserts = matchLength - pattern.Length;
            int startInserts = end - pattern.Length - middleInserts;
            int distance = best * 100 + startInserts + middleInserts;

            return new StrMatch(text.Substring(start, matchLength), distance, start, matchLength, text.Length, 1 - best / (double) pattern.Length);
        }
        #endregion
    }
}
