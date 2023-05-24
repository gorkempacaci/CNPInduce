using System;
using System.Collections.Generic;
namespace CNP.Language
{
  public class MathLib
  {
    public static List<LibraryProgram> MathLibrary = new List<LibraryProgram>()
    {
      new GenericMathPredicate2("lt", (a, b) => a<b),
      new GenericMathPredicate2("lte", (a, b) => a<=b),
      new GenericMathFunction2("+", (a, b) => a+b),
      new GenericMathFunction2("min", (a, b) => Math.Min(a, b)),
      new GenericMathFunction2("max", (a, b) => Math.Max(a, b)),
    };

    static MathLib()
    {

    }
  }
}

