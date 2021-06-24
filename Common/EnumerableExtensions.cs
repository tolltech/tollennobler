using System;
using System.Collections.Generic;

namespace Tolltech.Common
{
    public static class EnumerableExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey,TValue> dict, TKey key, Func<TKey, TValue> valueFunc)
        {
            if (dict.TryGetValue(key, out var val))
            {
                return val;
            }

            val = valueFunc(key);
            dict[key] = val;

            return val;
        }

        public static TValue SafeGet<TKey, TValue>(this IDictionary<TKey,TValue> dict, TKey key, TValue defaultValue = default)
        {
            return dict.TryGetValue(key, out var val) ? val : defaultValue;
        }
    }
}