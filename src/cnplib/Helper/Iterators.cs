using System;
using System.Collections.Generic;
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

        public static IEnumerable<T> Empty<T>() => new List<T>();

        public static void For<TSource>(this IEnumerable<TSource> source, Action<TSource, int> action)
        {
            IEnumerator<TSource> it = source.GetEnumerator();
            int i = 0;
            while (it.MoveNext())
            {
                action(it.Current, i++);
            }
        }

        public static IEnumerable<T> WhereAndNot<T>(this IEnumerable<T> sourceList, Func<T, bool> predicate, out IEnumerable<T> whereNot)
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
            whereNot = whereNotList;
            return whereList;
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

        public static IEnumerable<TSource> Clone<TSource>(this IEnumerable<TSource> source, FreeDictionary plannedParenthood) where TSource:IFreeContainer
        {
            return source.Select(e => (TSource)e.Clone(plannedParenthood));
        }
        
        public static (TSource, TSource) ToValueTuple2<TSource>(this IEnumerable<TSource> source)
        {
            var vals = Enumerable.ToArray(source);
            if (vals.Length!=2)
                throw  new Exception("ToValueTuple2: IEnumerable has more than 2 elements.");
            return ValueTuple.Create(vals[0], vals[1]);
        }
        public static (TSource, TSource, TSource) ToValueTuple3<TSource>(this IEnumerable<TSource> source)
        {
            var vals = Enumerable.ToArray(source);
            if (vals.Length!=3)
                throw  new Exception("ToValueTuple3: IEnumerable has more than 3 elements.");
            return ValueTuple.Create(vals[0], vals[1], vals[2]);
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

        public static IEnumerable<TResult> HeadAndTail<TResult>(TResult obj, IEnumerable<TResult> tail)
        {
            return Singleton(obj).Concat(tail);
        }
    }
}
