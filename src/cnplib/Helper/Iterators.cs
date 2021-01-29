using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CNP.Helper.EagerLinq;
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
    /// permutations([1]) = [[1]]
    /// permutations([1,2]) = [[1,2],[2,1]]
    /// permutations([1,2,3]) = [[1,2,3],[1,3,2],[2,1,3],[2,3,1],[3,1,2],[3,2,1]]
    /// TODO: Optimize
    /// </summary>
    public static IEnumerable<IEnumerable<TSource>> Permutations<TSource>(this IEnumerable<TSource> source)
    {
      int len = source.Count();
      if (len == 0)
        return Empty<IEnumerable<TSource>>();
      if (len == 1)
        return Singleton(source);
      List<IEnumerable<TSource>> result = new List<IEnumerable<TSource>>();
      for (int i = 0; i < len; i++)
      {
        var enumAtIndex = source.Skip(i);
        var head = enumAtIndex.First();
        var tail = source.Take(i).Concat(enumAtIndex.Skip(1));
        result.AddRange(tail.Permutations().Select(l => Singleton(head).Concat(l)));
      }
      return result;
    }

    public static IEnumerable<TTarget> Cartesian<TSource1, TSource2, TTarget>(this IEnumerable<TSource1> first,
        IEnumerable<TSource2> second, Func<TSource1, TSource2, TTarget> op)
    {
      List<TTarget> allElements = new List<TTarget>();
      foreach (TSource1 f in first)
      {
        foreach (TSource2 s in second)
        {
          allElements.Add(op(f, s));
        }
      }
      return allElements;
    }

    /// <summary>
    /// TODO: Replace with Enumerable.Empty
    /// </summary>
    public static IEnumerable<T> Empty<T>() => new List<T>();

    public static IEnumerable<TSource> Flatten<TSource>(this IEnumerable<IEnumerable<TSource>> lists)
    {
      return lists.Aggregate((l1, l2) => l1.Concat(l2));
    }

    public static void For<TSource>(this IEnumerable<TSource> source, Action<TSource, int> action)
    {
      IEnumerator<TSource> it = source.GetEnumerator();
      int i = 0;
      while (it.MoveNext())
      {
        action(it.Current, i++);
      }
    }

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

    // https://stackoverflow.com/questions/577590/pair-wise-iteration-in-c-sharp-or-sliding-window-enumerator
    public static IEnumerable<TResult> Pairwise<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TSource, TResult> resultSelector)
    {
      TSource previous = default(TSource);

      using (var it = source.GetEnumerator())
      {
        if (it.MoveNext())
          previous = it.Current;

        while (it.MoveNext())
          yield return resultSelector(previous, previous = it.Current);
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

    public static string ToMappingString<TKey, TValue>(this IDictionary<TKey, TValue> dict)
    {
      return string.Join(", ", dict.Select(kv => kv.Key.ToString() + ":" + kv.Value.ToString()));
    }

    public static IEnumerable<TSource> Clone<TSource>(this IEnumerable<TSource> source, TermReferenceDictionary plannedParenthood) where TSource : Term
    {
      return source.Select(e => (TSource)e.Clone(plannedParenthood));
    }

    public static (TSource, TSource) ToValueTuple2<TSource>(this IEnumerable<TSource> source)
    {
      var vals = Enumerable.ToArray(source);
      if (vals.Length != 2)
        throw new Exception("ToValueTuple2: IEnumerable has more than 2 elements.");
      return ValueTuple.Create(vals[0], vals[1]);
    }
    public static (TSource, TSource, TSource) ToValueTuple3<TSource>(this IEnumerable<TSource> source)
    {
      var vals = Enumerable.ToArray(source);
      if (vals.Length != 3)
        throw new Exception("ToValueTuple3: IEnumerable has more than 3 elements.");
      return ValueTuple.Create(vals[0], vals[1], vals[2]);
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
