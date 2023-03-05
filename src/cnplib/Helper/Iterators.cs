using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CNP.Language;

namespace CNP.Helper
{

  public static class Iterators
  {
    /// <summary>
    /// Makes a singleton list from a single object.
    /// </summary>
    public static IEnumerable<T> Singleton<T>(T obj)
    {
      yield return obj;
    }

    /// <summary>
    /// TODO: Replace with Enumerable.Empty
    /// </summary>
    public static IEnumerable<T> Empty<T>() => new List<T>();

    public static TSource[] Flatten<TSource>(this IEnumerable<IEnumerable<TSource>> lists)
    {
      return lists.SelectMany(l => l).ToArray();
    }

    /// <summary>
    /// Iterates through the given enumerable in order while giving indices. 
    /// </summary>
    public static void For<TSource>(this IEnumerable<TSource> source, Action<TSource, int> action)
    {
      IEnumerator<TSource> it = source.GetEnumerator();
      int i = 0;
      while (it.MoveNext())
      {
        action(it.Current, i++);
      }
    }

    /// <summary>
    /// Splits an enumeration into two, according to a given predicate: one containing elements the predicate holds, and one with the ones it doesn't hold.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sourceList"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static (IEnumerable<T>, IEnumerable<T>) WhereAndNot<T>(this IEnumerable<T> sourceList, Func<T, bool> predicate)
    {
      List<T> whereList = new List<T>();
      List<T> whereNotList = new List<T>();
      foreach (T e in sourceList)
      {
        if (predicate(e))
          whereList.Add(e);
        else
          whereNotList.Add(e);
      }
      return (whereList, whereNotList);
    }

    /// <summary>
    /// Iterates through the list in pairs (1, 2), (2, 3), (3, 4). Does not iterate if there is just one item.
    /// https://stackoverflow.com/questions/577590/pair-wise-iteration-in-c-sharp-or-sliding-window-enumerator
    /// </summary>
    public static IEnumerable<(T, T)> Pairwise<T>(this IEnumerable<T> source)
    {
      var previous = default(T);
      using (var it = source.GetEnumerator())
      {
        if (it.MoveNext())
          previous = it.Current;

        while (it.MoveNext())
          yield return (previous, previous = it.Current);
      }
    }

    public static List<TResult> Generate<TResult>(int count, Func<TResult> generator)
    {
      List<TResult> list = new List<TResult>(count);
      for (int i = 0; i < count; i++)
      {
        list.Add(generator());
      }
      return list;
    }

    public static HashSet<TSource> ToHashSet<TSource>(this IEnumerable<TSource> source)
    {
      return new HashSet<TSource>(source);
    }

    /// <summary>
    /// Returns true if all elements in IEnumerable are distinct.
    /// </summary>
    public static bool IsSet<TSource>(this IEnumerable<TSource> source)
    {
      HashSet<TSource> hs = new();
      foreach (TSource o in source)
        if (!hs.Add(o))
          return false;
      return true;
    }

    public static IEnumerable<TResult> HeadAndTail<TResult>(TResult obj, IEnumerable<TResult> tail)
    {
      return Singleton(obj).Concat(tail);
    }

    /// <summary>
    /// May return tail as null if there's only one element in the list.
    /// </summary>
    public static bool ToHeadAndTail<T>(IEnumerable<T> source, out T head, out IEnumerable<T> tail)
    {
      head = default(T);
      tail = System.Linq.Enumerable.Empty<T>();
      if (source.Any())
      {
        head = source.First();
        var rest = source.Skip(1);
        if (rest.Any())
        {
          tail = rest;
        }
        return true;
      }
      else return false;
    }

    public static string ToMappingString<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> dict)
    {
      return string.Join(", ", dict.Select(kv => kv.Key.ToString() + ":" + kv.Value.ToString()));
    }

    /// <summary>
    /// Checks equality for elements irrespective of their positions. Makes sure each element will be matched only with one element in the other array.
    /// </summary>
    public static bool EqualsAsSet<T>(this T[] aas, T[] bs)
    {
      if (aas.Length != bs.Length)
        return false;
      bool[] bmatched = new bool[bs.Length];
      for(int i=0; i<aas.Length; i++)
      {
        bool foundA = false;
        for (int k=0; k<bs.Length; k++)
        {
          if (aas[i].Equals(bs[k]) && !bmatched[k])
          {
            bmatched[k] = true;
            foundA = true;
            break;
          }
        }
        if (!foundA)
          return false;
      }
      return true;
    }


    /// <summary>
    /// Like SequenceEqual but outputs the index the sequences were not equal.
    /// </summary>
    /// <returns></returns>
    public static bool SequenceEqualPos<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, out int position)
    {
      position = 0;
      var firstEn = first.GetEnumerator();
      var secondEn = second.GetEnumerator();
      bool f = false, s = false;
      while ((f = firstEn.MoveNext()) && (s = secondEn.MoveNext()))
      {
        if (!EqualityComparer<TSource>.Default.Equals(firstEn.Current, secondEn.Current))
          return false;
        position++;
      }
      if (secondEn.MoveNext())
        return false;
      else return true;
    }
  

  public static bool EqualsAsDictionary<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dict1, IReadOnlyDictionary<TKey, TValue> dict2)
    {
      if (dict1.Count != dict2.Count)
        return false;
      return dict1.All(kv => dict2.ContainsKey(kv.Key) && dict2[kv.Key].Equals(kv.Value));
    }

    public static TValue GetOrAdd<TKey,TValue>(this IDictionary<TKey,TValue> source, TKey key, Func<TValue> valueGenerator)
    {
      return GetOrAdd(source, key, valueGenerator, out _);
    }

    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key, Func<TValue> valueGenerator, out bool added)
    {
      if (!source.TryGetValue(key, out TValue value))
      {
        value = valueGenerator();
        source.Add(key, value);
        added = true;
      }
      else added = false;
      return value;
    }

    public static void AddAll<TKey,TValue>(this IDictionary<TKey,TValue> dic, IEnumerable<KeyValuePair<TKey,TValue>> pairs)
    {
      foreach(var pair in pairs)
      {
        dic.Add(pair.Key, pair.Value);
      }
    }

    public static IEnumerable<TSource> GetCloned<TSource>(this IEnumerable<TSource> source, CloningContext cc) where TSource : ITerm
    {
      return source.Select(e => (TSource)e.Clone(cc));
    }

    /// <summary>
    /// Creates a ValueTuple2 from a source enumeration with exactly 2 elements.
    /// </summary>
    public static (TSource, TSource) ToValueTuple2<TSource>(this IEnumerable<TSource> source)
    {
      var en = source.GetEnumerator();
      if (!en.MoveNext())
        throw new Exception("ToValueTuple2: There are no elements in the source.");
      var el1 = en.Current;
      if (!en.MoveNext())
        throw new Exception("ToValueTuple2: There is no second element in the source.");
      var el2 = en.Current;
      if (en.MoveNext())
        throw new Exception("ToValueTuple2: There are more than 2 elements in the source.");
      return ValueTuple.Create(el1, el2);
    }

    /// <summary>
    /// Creates a ValueTuple3 from a source enumeration with exactly 3 elements.
    /// </summary>
    public static (TSource, TSource, TSource) ToValueTuple3<TSource>(this IEnumerable<TSource> source)
    {
      var en = source.GetEnumerator();
      if (!en.MoveNext())
        throw new Exception("ToValueTuple3: There are no elements in the source.");
      var el1 = en.Current;
      if (!en.MoveNext())
        throw new Exception("ToValueTuple3: There is no second element in the source.");
      var el2 = en.Current;
      if (!en.MoveNext())
        throw new Exception("ToValueTuple3: There is no third element in the source.");
      var el3 = en.Current;
      if (en.MoveNext())
        throw new Exception("ToValueTuple3: There are more than three elements in the source.");
      return ValueTuple.Create(el1, el2, el3);
    }
    public static IEnumerable<TResult> New<TResult, T1>(this IEnumerable<T1> source)
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), s));
    }
    public static IEnumerable<TResult> New<TResult>(this IEnumerable<object[]> source)
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), s));
    }
    public static IEnumerable<TResult> New<TResult, T1, T2>(this IEnumerable<Tuple<T1, T2>> source)
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), new object[] { s.Item1, s.Item2 }));
    }
    public static IEnumerable<TResult> New<TResult, T1, T2, T3>(this IEnumerable<Tuple<T1, T2, T3>> source)
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), new object[] { s.Item1, s.Item2 }));
    }
    public static IEnumerable<TResult> New<TResult, T1, T2, T3, T4>(this IEnumerable<Tuple<T1, T2, T3, T4>> source)
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), new object[] { s.Item1, s.Item2, s.Item3, s.Item4 }));
    }
    public static IEnumerable<TResult> New<TResult, T1, T2, T3, T4, T5>(this IEnumerable<Tuple<T1, T2, T3, T4, T5>> source)
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), new object[] { s.Item1, s.Item2, s.Item3, s.Item4, s.Item5 }));
    }
    public static IEnumerable<TResult> New<TResult, T1, T2, T3, T4, T5, T6>(this IEnumerable<Tuple<T1, T2, T3, T4, T5, T6>> source)
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), new object[] { s.Item1, s.Item2, s.Item3, s.Item4, s.Item5, s.Item6 }));
    }
    public static IEnumerable<TResult> New<TResult, T1, T2, T3, T4, T5, T6, T7>(this IEnumerable<Tuple<T1, T2, T3, T4, T5, T6, T7>> source)
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), new object[] { s.Item1, s.Item2, s.Item3, s.Item4, s.Item5, s.Item6, s.Item7 }));
    }
    public static IEnumerable<TResult> New<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(this IEnumerable<Tuple<T1, T2, T3, T4, T5, T6, T7, T8>> source) where T8 : struct
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), new object[] { s.Item1, s.Item2, s.Item3, s.Item4, s.Item5, s.Item6, s.Item7, s.Rest }));
    }
    public static IEnumerable<TResult> New<TResult, T1, T2>(this IEnumerable<ValueTuple<T1, T2>> source)
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), new object[] { s.Item1, s.Item2 }));
    }
    public static IEnumerable<TResult> New<TResult, T1, T2, T3>(this IEnumerable<ValueTuple<T1, T2, T3>> source)
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), new object[] { s.Item1, s.Item2 }));
    }
    public static IEnumerable<TResult> New<TResult, T1, T2, T3, T4>(this IEnumerable<ValueTuple<T1, T2, T3, T4>> source)
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), new object[] { s.Item1, s.Item2, s.Item3, s.Item4 }));
    }
    public static IEnumerable<TResult> New<TResult, T1, T2, T3, T4, T5>(this IEnumerable<ValueTuple<T1, T2, T3, T4, T5>> source)
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), new object[] { s.Item1, s.Item2, s.Item3, s.Item4, s.Item5 }));
    }
    public static IEnumerable<TResult> New<TResult, T1, T2, T3, T4, T5, T6>(this IEnumerable<ValueTuple<T1, T2, T3, T4, T5, T6>> source)
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), new object[] { s.Item1, s.Item2, s.Item3, s.Item4, s.Item5, s.Item6 }));
    }
    public static IEnumerable<TResult> New<TResult, T1, T2, T3, T4, T5, T6, T7>(this IEnumerable<ValueTuple<T1, T2, T3, T4, T5, T6, T7>> source)
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), new object[] { s.Item1, s.Item2, s.Item3, s.Item4, s.Item5, s.Item6, s.Item7 }));
    }
    public static IEnumerable<TResult> New<TResult, T1, T2, T3, T4, T5, T6, T7, T8>(this IEnumerable<ValueTuple<T1, T2, T3, T4, T5, T6, T7, T8>> source) where T8 : struct
    {
      return source.Select(s => (TResult)Activator.CreateInstance(typeof(TResult), new object[] { s.Item1, s.Item2, s.Item3, s.Item4, s.Item5, s.Item6, s.Item7, s.Rest }));
    }


  }
}
