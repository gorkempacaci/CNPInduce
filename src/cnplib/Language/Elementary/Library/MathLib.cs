using System;
using System.Collections.Generic;
namespace CNP.Language
{
  public class MathLib
  {
    public static LibraryProgram lt = new GenericMathPredicate2("lt", (a, b) => a < b);
    public static LibraryProgram leq = new GenericMathPredicate2("leq", (a, b) => a <= b);

    public static LibraryProgram plus = new GenericMathFunction2("+", (a, b) => a + b);
    public static LibraryProgram mul = new GenericMathFunction2("*", (a, b) => a * b);
    public static LibraryProgram min = new GenericMathFunction2("min", (a, b) => Math.Min(a, b));
    public static LibraryProgram max = new GenericMathFunction2("max", (a, b) => Math.Max(a, b));

    public static LibraryProgram increment = new GenericMathFunction1("increment", (n) => n + 1, ("n", "s"));

    public static List<LibraryProgram> MathLibrary = new List<LibraryProgram>()
    { lt, leq, plus, max, increment };

  }
}

