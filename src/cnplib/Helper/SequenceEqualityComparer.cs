﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CNP.Helper
{
  /// <summary>
  /// Considers sequences equal only if they're the same length and they have same elements in same locations
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class SequenceEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
  {
    public bool Equals([DisallowNull] IEnumerable<T> x, [DisallowNull] IEnumerable<T> y)
    {
      return x.SequenceEqual(y);
    }

    public int GetHashCode([DisallowNull] IEnumerable<T> obj)
    {
      T a = obj.FirstOrDefault();
      if (a is null)
        return 0;
      else return a.GetHashCode();
    }
  }
}

