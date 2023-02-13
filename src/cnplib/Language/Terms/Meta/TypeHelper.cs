using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CNP.Helper;
using CNP.Parsing;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
  public static class TypeHelper
  {
    //public static TypeStore<TComposedType> ParseListOfCompactedComposedTypes<TComposedType>(
    //    IEnumerable<string> compactStrs) where TComposedType : FunctionalValence
    //{
    //  var expandedStrs = compactStrs.SelectMany(expandAsteriskToInOut).Distinct();
    //  var types = expandedStrs.Select(sList => Parser.ParseFunctionalValence<TComposedType>(sList));
    //  return new TypeStore<TComposedType>(types);
    //}

    //public static TypeStore<Valence> ParseListOfCompactedProgramTypes(IEnumerable<string> compactStrs)
    //{
    //  var expandedStrs = compactStrs.SelectMany(expandAsteriskToInOut).Distinct();
    //  var types = expandedStrs.Select(s => Parser.ParseValence(s));
    //  return new TypeStore<Valence>(types);
    //}

  }

}
