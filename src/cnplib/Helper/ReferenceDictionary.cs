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

    public ReferenceDictionary(IEnumerable<KeyValuePair<TKey, TValue>> kvps)
    {
      foreach (var kvp in kvps)
      {
        this.Add(kvp.Key, kvp.Value);
      }
    }
  }

  public static class ReferenceDictionaryExtension
  {
    public static ReferenceDictionary<T_Key, T_Value> ToReferenceDictionary<TSource, T_Key, T_Value>(this IEnumerable<TSource> source, Func<TSource, T_Key> keyMaker, Func<TSource, T_Value> valueMaker)
    {
      ReferenceDictionary<T_Key, T_Value> refs = new ReferenceDictionary<T_Key, T_Value>();
      foreach (TSource el in source)
      {
        T_Key key = keyMaker(el);
        T_Value value = valueMaker(el);
        refs.Add(key, value);
      }
      return refs;
    }
  }

}