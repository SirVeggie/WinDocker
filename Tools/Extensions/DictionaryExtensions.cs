using System;
using System.Collections.Generic;

namespace Apprentice.Tools.Extensions {
    public static class DictionaryExtensions {

        /// <summary>Adds new if missing, otherwise update existing</summary>
        public static void Update<T, K>(this Dictionary<T, K> dict, T key, K value) {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }

        public static Dictionary<T, K> InitializeKeys<T, K>(this Dictionary<T, K> dict, List<T> keys) {
            foreach (T key in keys) {
                if (!dict.ContainsKey(key)) {
                    dict.Add(key, default);
                }
            }

            return dict;
        }

        public static Dictionary<T, K> InitializeKeys<T, K>(this Dictionary<T, K> dict, List<T> keys, Func<K> initializer) {
            if (initializer == null)
                throw new ArgumentNullException("Initializer was null");
            foreach (T key in keys) {
                if (!dict.ContainsKey(key)) {
                    dict.Add(key, initializer.Invoke());
                }
            }

            return dict;
        }
    }
}
