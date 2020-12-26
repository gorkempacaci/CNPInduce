using System;
using System.Collections.Generic;

namespace CNP.Helper
{

  /// <summary>
  /// A dictionary that only identifies objects by their reference.
  /// </summary>
  public class ReferenceDictionary<TKey, TValue> : Dictionary<TKey, TValue>
  {
    public ReferenceDictionary() : base(ReferenceEqualityComparer.Instance as IEqualityComparer<TKey>)
    {

    }

    /// <summary>
    /// Returns a shallow copy of the dictionary where term referenes are shared with this one.
    /// </summary>
    /// <returns></returns>
    public ReferenceDictionary<TKey, TValue> Copy()
    {
      ReferenceDictionary<TKey, TValue> fd = new ReferenceDictionary<TKey, TValue>();

      foreach (var kvp in this)
      {
        fd.Add(kvp.Key, kvp.Value);
      }
      return fd;
    }

    public TValue GetOrAdd(TKey key, Func<TValue> valueGenerator)
    {
      if (!TryGetValue(key, out TValue value))
      {
        value = valueGenerator();
        Add(key, value);
      }
      return value;
    }

    public ReferenceDictionary(IEnumerable<KeyValuePair<TKey, TValue>> kvps)
    {
      foreach (var kvp in kvps)
      {
        this.Add(kvp.Key, kvp.Value);
      }
    }
  }

}