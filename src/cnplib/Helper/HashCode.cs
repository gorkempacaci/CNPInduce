using System;
using System.Linq;
using System.Collections.Generic;

namespace CNP.Helper
{
    public static class HashCode
    {
        /// <summary>
        /// Not commutative.
        /// </summary>
        public static int Combined(object o1, object o2)
        {
            return CombinedHashes(o1.GetHashCode(), o2.GetHashCode());
        }
        /// <summary>
        /// Not commutative.
        /// </summary>
        public static int Combined<T1, T2>(T1 o1, T2 o2)
        {
            return CombinedHashes(o1.GetHashCode(), o2.GetHashCode());
        }
        /// <summary>
        /// Not commutative.
        /// </summary>
        public static int CombinedHashes(int h1, int h2)
        {
            // https://stackoverflow.com/a/37449594
            return ((h1 << 5) + h1) ^ h2;
        }

        public static int OfDictionary<TKey, TValue>(IDictionary<TKey, TValue> dic)
        {
            return dic.OrderBy(kvp => kvp.Key)
                      .Select(kvp => HashCode.Combined(kvp.Key, kvp.Value))
                      .Aggregate(HashCode.CombinedHashes);
        }
    }
}
