using System;
using System.Collections.Generic;
using System.Text;
using Lazy = System.Linq.Enumerable;

namespace CNP.Helper.EagerLinq
{
    public static class Enumerable
    {
        public static bool Any<TSource>(this IEnumerable<TSource> source)
        {
            return Lazy.Any(source);
        }
        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return Lazy.Any(source, predicate);
        }
        public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return Lazy.ToList(Lazy.Where(source, predicate));
        }
        public static IEnumerable<TResult> Select<TSource,TResult>(this IEnumerable<TSource> source, Func<TSource,TResult> selector)
        {
            return Lazy.ToList(Lazy.Select(source, selector));
        }
        public static IEnumerable<TResult> SelectMany<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            return Lazy.ToList(Lazy.SelectMany(source, selector));
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
        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            return Lazy.Count(source);
        }
        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return Lazy.Count(source, predicate);
        }
        public static IEnumerable<int> Range(int start, int count)
        {
            return Lazy.ToList(Lazy.Range(start, count));
        }
        public static IEnumerable<TSource> Reverse<TSource>(this IEnumerable<TSource> source)
        {
            return Lazy.ToList(Lazy.Reverse(source));
        }
        public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> other)
        {
            return Lazy.ToList(Lazy.Concat(source, other));
        }
        public static IEnumerable<TResult> Zip<TSource1, TSource2, TResult>(this IEnumerable<TSource1> source1, IEnumerable<TSource2> source2, Func<TSource1,TSource2,TResult> zipper)
        {
            return Lazy.ToList(Lazy.Zip(source1, source2, zipper));
        }
        public static IEnumerable<TSource> Take<TSource>(this IEnumerable<TSource> source, int count)
        {
            return Lazy.ToList(Lazy.Take(source, count));
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
        public static int Max<TSource>(this IEnumerable<TSource> source, Func<TSource,int> selector)
        {
            return Lazy.Max(source, selector);
        }
        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            return Lazy.SequenceEqual(first, second);
        }
    }
    
}
