using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Lazy = System.Linq.Enumerable;

namespace CNP.Helper.EagerLinq
{
  public static class Enumerable
  {
    public static TSource Aggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> f)
    {
      return Lazy.Aggregate(source, f);
    }
    public static bool Any<TSource>(this IEnumerable<TSource> source)
    {
      return Lazy.Any(source);
    }
    public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      return Lazy.Any(source, predicate);
    }
    public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
    {
      return Lazy.ElementAt(source, index);
    }
    public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
    {
      return Lazy.ElementAtOrDefault(source, index);
    }
    public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      return Lazy.ToList(Lazy.Where(source, predicate));
    }
    public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
    {
      return Lazy.ToList(Lazy.Select(source, selector));
    }
    public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
    {
      return Lazy.ToList(Lazy.SelectMany(source, selector));
    }

    public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(this IEnumerable<TSource> source,
        Func<TSource, IEnumerable<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
      return Lazy.ToList(Lazy.SelectMany(source, collectionSelector, resultSelector));
    }
    public static TSource First<TSource>(this IEnumerable<TSource> source)
    {
      return Lazy.First(source);
    }
    public static Dictionary<TKey, TValue> ToDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
    {
      return Lazy.ToDictionary(source, keySelector, valueSelector);
    }
    public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
    {
      return Lazy.Contains(source, value);
    }
    public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      return Lazy.All(source, predicate);
    }
    public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> other)
    {
      return Lazy.ToList(Lazy.Concat(source, other));
    }
    public static int Count<TSource>(this IEnumerable<TSource> source)
    {
      return Lazy.Count(source);
    }
    public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
      return Lazy.Count(source, predicate);
    }
    public static IEnumerable<TSource> Distinct<TSource>(this IEnumerable<TSource> source)
    {
      return Lazy.ToList(Lazy.Distinct(source));
    }
    public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> second)
    {
      return Lazy.ToList(Lazy.Except(source, second));
    }
    public static IEnumerable<System.Linq.IGrouping<TKey, TSource>> GroupBy<TKey, TSource>(this IEnumerable<TSource> source,
        Func<TSource, TKey> condition)
    {
      return Lazy.ToList(Lazy.GroupBy(source, condition));
    }
    public static IEnumerable<int> Range(int start, int count)
    {
      return Lazy.ToList(Lazy.Range(start, count));
    }
    public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source)
    {
      return Lazy.ToList(Lazy.Reverse(source));
    }
    public static IEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
      return Lazy.ToList(Lazy.OrderBy(source, keySelector));
    }
    public static IEnumerable<TSource> OrderByDescending<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
      return Lazy.ToList(Lazy.OrderByDescending(source, keySelector));
    }
    public static IEnumerable<TResult> Zip<TSource1, TSource2, TResult>(this IEnumerable<TSource1> source1, IEnumerable<TSource2> source2, Func<TSource1, TSource2, TResult> zipper)
    {
      return Lazy.ToList(Lazy.Zip(source1, source2, zipper));
    }
    public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
    {
      return Lazy.ToList(Lazy.Take(source, count));
    }
    public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
    {
      return Lazy.ToArray(source);
    }
    public static IEnumerable<TSource> Skip<TSource>(this IEnumerable<TSource> source, int count)
    {
      return Lazy.ToList(Lazy.Skip(source, count));
    }
    public static TSource Last<TSource>(this IEnumerable<TSource> source)
    {
      return Lazy.Last(source);
    }
    public static List<TSource> ToList<TSource>(IEnumerable<TSource> source)
    {
      return Lazy.ToList(source);
    }
    public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count)
    {
      return Lazy.ToList(Lazy.Repeat(element, count));
    }
    public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
    {
      return Lazy.Max(source, selector);
    }
    public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
    {
      return Lazy.SequenceEqual(first, second);
    }
    public static bool SequenceEqualPos<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, out int position)
    {
      position = 0;
      var firstEn = first.GetEnumerator();
      var secondEn = second.GetEnumerator();
      bool f = false, s = false;
      while((f=firstEn.MoveNext()) && (s=secondEn.MoveNext()))
      {
        if (!EqualityComparer<TSource>.Default.Equals(firstEn.Current, secondEn.Current))
          return false;
        position++;
      }
      if (secondEn.MoveNext())
        return false;
      else return true;
    }
    public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> equalityComparer)
    {
      return Lazy.SequenceEqual(first, second, equalityComparer);
    }
  }

}
