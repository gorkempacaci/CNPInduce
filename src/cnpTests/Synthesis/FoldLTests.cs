using System.Collections.Generic;
using System.Linq;
using CNP;
using CNP.Helper;
using CNP.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Synthesis
{
  [TestClass]
  public class FoldLTests : TestBase
  {

    [TestMethod]
    public void DecomposeFoldL()
    {
      NameVarBindings nvb = new();
      NameVar b0 = nvb.AddNameVar("b0");
      NameVar @as = nvb.AddNameVar("as");
      NameVar b = nvb.AddNameVar("b");
      var names = new NameVar[] { b0, @as, b };
      var tups = new ITerm[][] { new ITerm[] { list(), list(1, 2, 3), list(3, 2, 1) } };
      var foldrel = new AlphaRelation(names, tups);
      var freeFact = new FreeFactory();
      FoldL.UnFoldL(foldrel, (0, 1, 2), freeFact, out var pTuples, out var qTuples);
      var stringer = new PrettyStringer(nvb);
      var pTuplesString = stringer.Visit(pTuples, FoldL.FoldLValences.RecursiveCaseNames);
      var qTuplesString = stringer.Visit(qTuples, FoldL.FoldLValences.BaseCaseNames);
      Assert.AreEqual("{{a:1, b:[], ab:F0}, {a:2, b:F0, ab:F1}, {a:3, b:F1, ab:F2}}", pTuplesString, "Recursive case tuples should match");
      Assert.AreEqual("{{a:F2, b:[3, 2, 1]}}", qTuplesString, "Base case tuples should match.");
    }

    [TestMethod]
    public void Reverse3()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[], as:[1,2,3], b:[3,2,1]}}";
      assertFirstResultFor(typeStr, atusStr, "foldl(cons, id)");
    }

  }
}
