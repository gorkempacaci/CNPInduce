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

        public static IEnumerable<TResult> New<TResult, T1>(this IEnumerable<T1> source)
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), s));
        }        
        public static IEnumerable<TResult> New<TResult>(this IEnumerable<object[]> source)
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), s));
        }
        public static IEnumerable<TResult> New<TResult,T1,T2>(this IEnumerable<Tuple<T1,T2>> source)
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), new object[]{s.Item1,s.Item2}));
        }
        public static IEnumerable<TResult> New<TResult,T1,T2,T3>(this IEnumerable<Tuple<T1,T2,T3>> source)
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), new object[]{s.Item1,s.Item2}));
        }
        public static IEnumerable<TResult> New<TResult,T1,T2,T3,T4>(this IEnumerable<Tuple<T1,T2,T3,T4>> source)
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), new object[]{s.Item1,s.Item2,s.Item3,s.Item4}));
        }
        public static IEnumerable<TResult> New<TResult,T1,T2,T3,T4,T5>(this IEnumerable<Tuple<T1,T2,T3,T4,T5>> source)
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), new object[]{s.Item1,s.Item2,s.Item3,s.Item4,s.Item5}));
        }
        public static IEnumerable<TResult> New<TResult,T1,T2,T3,T4,T5,T6>(this IEnumerable<Tuple<T1,T2,T3,T4,T5,T6>> source)
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), new object[]{s.Item1,s.Item2,s.Item3,s.Item4,s.Item5,s.Item6}));
        }
        public static IEnumerable<TResult> New<TResult,T1,T2,T3,T4,T5,T6,T7>(this IEnumerable<Tuple<T1,T2,T3,T4,T5,T6,T7>> source)
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), new object[]{s.Item1,s.Item2,s.Item3,s.Item4,s.Item5,s.Item6,s.Item7}));
        }
        public static IEnumerable<TResult> New<TResult,T1,T2,T3,T4,T5,T6,T7,T8>(this IEnumerable<Tuple<T1,T2,T3,T4,T5,T6,T7,T8>> source) where T8:struct
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), new object[]{s.Item1,s.Item2,s.Item3,s.Item4,s.Item5,s.Item6,s.Item7,s.Rest}));
        }
        public static IEnumerable<TResult> New<TResult,T1,T2>(this IEnumerable<ValueTuple<T1,T2>> source)
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), new object[]{s.Item1,s.Item2}));
        }
        public static IEnumerable<TResult> New<TResult,T1,T2,T3>(this IEnumerable<ValueTuple<T1,T2,T3>> source)
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), new object[]{s.Item1,s.Item2}));
        }
        public static IEnumerable<TResult> New<TResult,T1,T2,T3,T4>(this IEnumerable<ValueTuple<T1,T2,T3,T4>> source)
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), new object[]{s.Item1,s.Item2,s.Item3,s.Item4}));
        }
        public static IEnumerable<TResult> New<TResult,T1,T2,T3,T4,T5>(this IEnumerable<ValueTuple<T1,T2,T3,T4,T5>> source)
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), new object[]{s.Item1,s.Item2,s.Item3,s.Item4,s.Item5}));
        }
        public static IEnumerable<TResult> New<TResult,T1,T2,T3,T4,T5,T6>(this IEnumerable<ValueTuple<T1,T2,T3,T4,T5,T6>> source)
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), new object[]{s.Item1,s.Item2,s.Item3,s.Item4,s.Item5,s.Item6}));
        }
        public static IEnumerable<TResult> New<TResult,T1,T2,T3,T4,T5,T6,T7>(this IEnumerable<ValueTuple<T1,T2,T3,T4,T5,T6,T7>> source)
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), new object[]{s.Item1,s.Item2,s.Item3,s.Item4,s.Item5,s.Item6,s.Item7}));
        }
        public static IEnumerable<TResult> New<TResult,T1,T2,T3,T4,T5,T6,T7,T8>(this IEnumerable<ValueTuple<T1,T2,T3,T4,T5,T6,T7,T8>> source) where T8:struct
        {
            return source.Select(s => (TResult) Activator.CreateInstance(typeof(TResult), new object[]{s.Item1,s.Item2,s.Item3,s.Item4,s.Item5,s.Item6,s.Item7,s.Rest}));
        }
    }
}
