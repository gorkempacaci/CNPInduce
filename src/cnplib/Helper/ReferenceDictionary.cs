using System.Collections.Generic;

namespace CNP.Helper
{

    /// <summary>
    /// A dictionary that only identifies objects by their reference.
    /// </summary>
    public class ReferenceDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public ReferenceDictionary() : base(ReferenceEqualityComparer<TKey>.Instance)
        {
        }

        /// <summary>
        /// Returns a shallow copy of the dictionary where term referenes are shared with this one.
        /// </summary>
        /// <returns></returns>
        public ReferenceDictionary<TKey,TValue> Copy()
        {
            ReferenceDictionary<TKey, TValue> fd = new ReferenceDictionary<TKey, TValue>();
            
            foreach(var kvp in this)
            {
                fd.Add(kvp.Key, kvp.Value);
            }
            return fd;
        }
    }
    /// <summary>
    /// Only matches objects if they have the same reference.
    /// </summary>
    public class ReferenceEqualityComparer<TKey> : IEqualityComparer<TKey>
    {
        public bool Equals(TKey x, TKey y)
        {
            return object.ReferenceEquals(x, y);
        }

        public int GetHashCode(TKey obj)
        {
            return obj.GetHashCode();
        }

        private ReferenceEqualityComparer() { }

        static ReferenceEqualityComparer()
        {
            Instance = new ReferenceEqualityComparer<TKey>();
        }
        public static ReferenceEqualityComparer<TKey> Instance { get; private set; }
    }
}