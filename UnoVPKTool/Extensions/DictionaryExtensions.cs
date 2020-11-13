using System.Collections.Generic;

namespace UnoVPKTool.Extensions
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Returns the value at the specified key, creating a new V if it doesn't exist.
        /// </summary>
        /// <typeparam name="K">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="V">The type of the values in the dictionary.</typeparam>
        /// <param name="dict">The dictionary to use.</param>
        /// <param name="key">The key to retrieve.</param>
        /// <returns></returns>
        public static V SafeAccess<K, V>(this Dictionary<K, V> dict, K key)
            where K : notnull
            where V : new()
        {
            if (!dict.ContainsKey(key)) dict[key] = new V();

            return dict[key];
        }

        /// <summary>
        /// Sets the value at the specified key using the given value.
        /// </summary>
        /// <typeparam name="K">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="V">The type of the values in the dictionary.</typeparam>
        /// <param name="dict">The dictionary to use.</param>
        /// <param name="key">The key to retrieve.</param>
        /// <param name="value">The value to use.</param>
        /// <returns></returns>
        public static V SafeAccess<K, V>(this Dictionary<K, V> dict, K key, V value)
            where K : notnull
        {
            if (!dict.ContainsKey(key)) dict[key] = value;

            return dict[key];
        }
    }
}