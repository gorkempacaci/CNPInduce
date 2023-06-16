using System;
using System.Collections.Generic;
using CNP;
using CNP.Helper;
using System.Linq;
using CNP.Language;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Synthesis
{
  [TestClass]
  public class FoldRTests : TestBase
  {
    [TestMethod]
    public void DecomposeFoldR()
    {
      NameVarBindings nvb = new();
      var freeFact = new FreeFactory();
      NameVar b0 = nvb.AddNameVar("b0");
      NameVar @as = nvb.AddNameVar("as");
      NameVar b = nvb.AddNameVar("b");
      var names = new NameVar[] { b0, @as, b };
      var tups = new ITerm[][] { new ITerm[] { list(), list(1, 2, 3), list(1, 2, 3) } };
      var foldrel = new AlphaRelation(names, tups);
      ValenceVar vv = new ValenceVar(new[] { b0, @as }, new[] { b });
      ObservedProgram obs = new ObservedProgram(foldrel, vv, 2, 0, ObservedProgram.Constraint.None);
      //ProgramEnvironment env = new(obs, nvb, freeFact);
      FoldR.UnFoldR(foldrel, (0, 1, 2), freeFact, out var pTuples);
      var stringer = new PrettyStringer(nvb);
      var pTuplesString = stringer.Visit(pTuples, FoldR.Valences.RecursiveCaseNames);
      Assert.AreEqual("{{a:3, b:[], ab:F1}, {a:2, b:F1, ab:F0}, {a:1, b:F0, ab:[1, 2, 3]}}", pTuplesString, "Recursive case tuples should match");
    }

    [TestMethod]
    public void Append()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[4,5,6], as:[1,2,3], b:[1,2,3,4,5,6]}}";
      assertFirstResultFor(typeStr, atusStr, "foldr(cons)");
    }
    [TestMethod]
    public void AppendToUnit()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[4], as:[1,2,3], b:[1,2,3,4]}}";
      assertFirstResultFor(typeStr, atusStr, "foldr(cons)");
    }

    [TestMethod]
    public void AppendToEmpty()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[], as:[1,2,3], b:[1,2,3]}}";
      assertFirstResultFor(typeStr, atusStr, "foldr(cons)");
    }

    [TestMethod]
    public void List_identity()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[], as:[1,2,3], b:[1,2,3]}}";
      assertFirstResultFor(typeStr, atusStr, "foldr(cons)");
    }

    
    //[TestMethod]
    //public void Reverse2FoldR()
    //{
    //  string typeStr = "{as:in, bs:out}";
    //  string atusStr = "{{as:[], bs:[]},{as:[1,2,3], bs:[3,2,1]}}";
    //  assertFirstResultFor(typeStr, atusStr, "proj(and(const(b0, []), foldl(cons)), {as->as, b->bs})", 4);
    //}


    public void FoldL_Proj_FoldL_Plus()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atus = "{{b0:0, as:[[1,1,1]], b:3}, {b0:0, as:[[1,2], [3,4]], b:10}, {b0:0, as:[[1,2,3], [4,5,6], [7,8,9]], b:45}}";
      assertFirstResultFor(typeStr, atus, "foldl(proj(foldl(+), {as->a, b0->b, b->ab}))", 4);
    }

    [TestMethod]
    public void Flatten3()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atus = "{{b0:[], as:[[1,2]], b:[1, 2]}, {b0:[], as:[[1,2], [3,4], [5,6]], b:[1,2,3,4,5,6]}}";
      //string atus = "{{b0:[], as:[[1,2], [3,4], [5,6]], b:[1,2,3,4,5,6]}}";
      assertFirstResultFor(typeStr, atus, "foldr(proj(foldr(cons), {as->a, b0->b, b->ab}))", 4);
    }

    [TestMethod]
    public void Flatten2()
    {
      string typeStr = "{as:in, bs:out}";
      //string atus = "{{as:[], bs:[]}, {as:[[1],[2]], bs:[1, 2]}, {as:[[1,2], [3,4], [5,6]], bs:[1,2,3,4,5,6]}}";
      string atus = "{{as:[[1,2], [3,4], [5,6]], bs:[1,2,3,4,5,6]}, {as:[[1]], bs:[1]}, {as:[[1,2]], bs:[1,2]}}";
      assertFirstResultFor(typeStr, atus, "proj(and(const(b0, []), foldr(proj(foldr(cons), {as->a, b0->b, b->ab}))), {as->as, b->bs})", 6, 1);
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
      string atusStr = "{{a0:0, as:[1,3], b:[1,2]}, {a0:5, as:[3,1], b:[2,1]}}";
      assertNoResultFor(typeStr, atusStr, 1);
    }

    [TestMethod]
    public void Identity_Fails_If_Initial_Is_Wrong_2()
    {
      string typeStr = "{a0:in, as:in, b:out}";
      string atusStr = "{{a0:[], as:[1,2,3], b:[1,2,3,4]}}";
      assertNoResultFor(typeStr, atusStr, 1);
    }

    [TestMethod]
    public void Identity_Fails_If_Initial_Is_Wrong_3()
    {
      string typeStr = "{a0:in, as:in, b:out}";
      string atusStr = "{{a0:[], as:[1], b:[1,2]}}";
      assertNoResultFor(typeStr, atusStr, 1);
    }

    [TestMethod]
    public void Identity_Fails_If_Initial_Is_Wrong_4()
    {
      string typeStr = "{a0:in, as:in, b:out}";
      string atusStr = "{{a0:[], as:[], b:[1]}}";
      assertNoResultFor(typeStr, atusStr, 1);
    }

    [TestMethod]
    public void AppendFail_1()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[4,5,6], as:[1,2,3], b:[1,2,3]}, {b0:[1], as:[2], b:[1,3]}}";
      assertNoResultFor(typeStr, atusStr, 2);
    }
  }
}
