using System;
using CNP.Helper;
using CNP.Helper.EagerLinq;
using System.Collections.Generic;
namespace CNP.Helper
{
  public static class Mathes
  {
    /// <summary>
    /// Subsets of the given set, including the empty set, and the whole set itself. 
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Subsets<T>(IEnumerable<T> items)
    {
      if (!items.Any())
        return Iterators.Singleton(Iterators.Empty<T>());
      return Iterators.Singleton(Iterators.Empty<T>()).Concat(Combinations(items));
    }

    /// <summary>
    /// All combinatios (n 1) ++ ... ++ (n n) for a given n-set. Order may be unintuitive due to implementation.
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Combinations<T>(IEnumerable<T> items)
    {
      if (!items.Any())
        return Iterators.Empty<IEnumerable<T>>();
      Iterators.ToHeadAndTail(items, out T head, out IEnumerable<T> tail);
      var headInLL = new[] { new[] { head } };
      var combTail = Combinations(tail);
      var cartHeadCombTail = Cartesian(Iterators.Singleton(head), combTail, Iterators.HeadAndTail);
      return headInLL.Concat(combTail).Concat(cartHeadCombTail);
    }

    /// <summary>
    /// Returns all k-length combinations of elements in the list, without repeating. If [a,b] appeared, [b,a] won't.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    public static IEnumerable<IEnumerable<T>> Combinations<T>(IEnumerable<T> items, int k)
    {
      int length = items.Count();
      if (length < k)
        throw new ArgumentOutOfRangeException("k-combinations of a list where the length are less than k is not possible.");
      if (k == 1)
        return items.Select(Iterators.Singleton);
      Iterators.ToHeadAndTail(items, out T head, out IEnumerable<T> tail);
      var tailCombsKminus = Mathes.Combinations(tail, k - 1);
      var combs = Mathes.Cartesian(Iterators.Singleton(head), tailCombsKminus, (a, b) => Iterators.HeadAndTail(a, b));
      if (length - 1 >= k)
      {
        var tailsOwnCombs = Mathes.Combinations(tail, k);
        combs = combs.Concat(tailsOwnCombs);
      }
      return combs;
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
        return Iterators.Empty<IEnumerable<TSource>>();
      if (len == 1)
        return Iterators.Singleton(source);
      List<IEnumerable<TSource>> result = new List<IEnumerable<TSource>>();
      for (int i = 0; i < len; i++)
      {
        var enumAtIndex = source.Skip(i);
        var head = enumAtIndex.First();
        var tail = source.Take(i).Concat(enumAtIndex.Skip(1));
        result.AddRange(tail.Permutations().Select(l => Iterators.Singleton(head).Concat(l)));
      }
      return result;
    }

    /// <summary>
    /// Pairs each element in the first list with each element in the other list, and returns the constructed elements.
    /// </summary>
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
    /// Pairs each element in the first list with each element in the other list, and executes the given action.
    /// </summary>
    public static void Cartesian<TSource1, TSource2>(this IEnumerable<TSource1> first,
        IEnumerable<TSource2> second, Action<TSource1, TSource2> op)
    {
      foreach (TSource1 f in first)
      {
        foreach (TSource2 s in second)
        {
          op(f, s);
        }
      }
    }



    public static IEnumerable<TTarget> Cartesian3<TSource1, TSource2, TSource3, TTarget>(this IEnumerable<TSource1> first, IEnumerable<TSource2> second, IEnumerable<TSource3> third, Func<TSource1, TSource2, TSource3, TTarget> op)
    {
      List<TTarget> allElements = new List<TTarget>();
      foreach (TSource1 f in first)
      {
        foreach (TSource2 s in second)
        {
          foreach (TSource3 t in third)
          {
            allElements.Add(op(f, s, t));
          }
        }
      }
      return allElements;
    }
  }
}
