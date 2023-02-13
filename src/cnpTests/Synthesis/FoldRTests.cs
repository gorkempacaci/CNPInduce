using System;
using System.Collections.Generic;
using CNP;
using CNP.Helper;
using System.Linq;
using CNP.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Synthesis
{
  [TestClass]
  public class FoldRTests : TestBase
  {
    [TestMethod]
    public void DecomposeFoldR()
    {
      NameVarBindings nvb = new();
      NameVar b0 = nvb.AddNameVar("b0");
      NameVar @as = nvb.AddNameVar("as");
      NameVar b = nvb.AddNameVar("b");
      var names = new NameVar[] { b0, @as, b };
      var tups = new ITerm[][] { new ITerm[] { list(), list(1, 2, 3), list(1, 2, 3) } };
      var foldrel = new AlphaRelation(names, tups);
      var freeFact = new FreeFactory();
      FoldR.UnFoldR(foldrel, (0, 1, 2), freeFact, out var pTuples, out var qTuples);
      var stringer = new PrettyStringer(nvb);
      var pTuplesString = stringer.PrettyString(pTuples, FoldR.FoldRValences.RecursiveCaseNames);
      var qTuplesString = stringer.PrettyString(qTuples, FoldR.FoldRValences.BaseCaseNames);
      Assert.AreEqual("{{a:1, b:F0, ab:[1, 2, 3]}, {a:2, b:F1, ab:F0}, {a:3, b:F2, ab:F1}}", pTuplesString, "Recursive case tuples should match");
      Assert.AreEqual("{{a:[], b:F2}}", qTuplesString, "Base case tuples should match.");
    }

    [TestMethod]
    public void Append()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[4,5,6], as:[1,2,3], b:[1,2,3,4,5,6]}}";
      assertFirstResultFor(typeStr, atusStr, "foldr(cons, id)", "append");
    }
    [TestMethod]
    public void AppendToUnit()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[4], as:[1,2,3], b:[1,2,3,4]}}";
      assertFirstResultFor(typeStr, atusStr, "foldr(cons, id)", "append");
    }

    [TestMethod]
    public void AppendToEmpty()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[], as:[1,2,3], b:[1,2,3]}}";
      assertFirstResultFor(typeStr, atusStr, "foldr(cons, id)", "append");
    }

    [TestMethod]
    public void List_identity()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[], as:[1,2,3], b:[1,2,3]}}";
      assertFirstResultFor(typeStr, atusStr, "foldr(cons, id)", "list_id");
    }
  }

  // [TestClass]
  // public class FoldRProjFoldR : TestBase
  // {
  //     [TestMethod]
  //     public void Flatten()
  //     {
  //         string typeStr = "{a0:in, as:in, b:out}";
  //         string atusStr = "{{a0:[], as:[[1,2,3], [4,5,6], [7,8,9]], b:[1,2,3,4,5,6,7,8,9]}}";
  //         assertSingleResultFor(typeStr, atusStr, foldr(foldr(cons, id), id));
  //     }
  //
  //     [TestMethod]
  //     public void Reverse2()
  //     {
  //         string typeStr = "{as:in, bs:out}";
  //         string atusStr = "{{as:[1,2,3], bs:[3,2,1]}}";
  //         assertSingleResultFor(typeStr, atusStr, proj(foldl(cons, id), {as,b->bs
  //         }), "reverse2");
  //     }
  // }

  [TestClass]
  public class FoldNegatives : TestBase
  {
    [TestMethod]
    public void Identity_Fails_If_Initial_Is_Wrong()
    {
      string typeStr = "{a0:in, as:in, b:out}";
      string atusStr = "{{a0:0, as:[1,2,3], b:[1,2,3]}, {a0:0, as:[3,2,1], b:[3,2,1]}}";
      assertNoResultFor(typeStr, atusStr, "neg id");
    }

    [TestMethod]
    public void Identity_Fails_If_Initial_Is_Wrong_2()
    {
      string typeStr = "{a0:in, as:in, b:out}";
      string atusStr = "{{a0:[], as:[1,2,3], b:[1,2,3,4]}}";
      assertNoResultFor(typeStr, atusStr, "neg id");
    }

    [TestMethod]
    public void Identity_Fails_If_Initial_Is_Wrong_3()
    {
      string typeStr = "{a0:in, as:in, b:out}";
      string atusStr = "{{a0:[], as:[1], b:[1,2]}}";
      assertNoResultFor(typeStr, atusStr, "neg id");
    }

    [TestMethod]
    public void Identity_Fails_If_Initial_Is_Wrong_4()
    {
      string typeStr = "{a0:in, as:in, b:out}";
      string atusStr = "{{a0:[], as:[], b:[1]}}";
      assertNoResultFor(typeStr, atusStr, "neg id");
    }

    [TestMethod]
    public void AppendFail_1()
    {
      string typeStr = "{a0:in, as:in, b:out}";
      string atusStr = "{{a0:[4,5,6], as:[1,2,3], b:[1,2,3]}}";
      assertNoResultFor(typeStr, atusStr, "neg append");
    }
  }
}
