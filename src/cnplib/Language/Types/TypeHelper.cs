using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using CNP.Helper;
using CNP.Parsing;

namespace CNP.Language
{
  public static class TypeHelper
  {
    public static TypeStore<TComposedType> ParseListOfCompactedComposedTypes<TComposedType>(
        IEnumerable<string> compactStrs) where TComposedType : ComposedType
    {
      var expandedStrs = compactStrs.SelectMany(expandAsteriskToInOut).Distinct();
      var types = expandedStrs.Select(sList => Parser.ParseOperatorType<TComposedType>(sList));
      return new TypeStore<TComposedType>(types);
    }

    public static TypeStore<ProgramType> ParseListOfCompactedProgramTypes(IEnumerable<string> compactStrs)
    {
      var expandedStrs = compactStrs.SelectMany(expandAsteriskToInOut).Distinct();
      var types = expandedStrs.Select(s => Parser.ParseProgramType(s));
      return new TypeStore<ProgramType>(types);
    }

    private static IEnumerable<string> expandAsteriskToInOut(string c)
    {
      int i = c.IndexOf('*');
      if (i == -1)
      {
        return Iterators.Singleton(c);
      }
      else
      {
        string pr = c.Substring(0, i);
        string po = c.Substring(i + 1);
        var withIn = expandAsteriskToInOut(pr + "in" + po);
        var withOut = expandAsteriskToInOut(pr + "out" + po);
        return Enumerable.Concat(withIn, withOut);
      }
    }
  }

}
