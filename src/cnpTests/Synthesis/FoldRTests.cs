using System;
using System.Collections.Generic;
using CNP;
using CNP.Helper;
using CNP.Helper.EagerLinq;
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
      var atusP = new List<AlphaTuple>();
      var atusQ = new List<AlphaTuple>();
      var namesP = new NameVarDictionary();
      var namesQ = new NameVarDictionary();
      FoldR.unfoldFoldrToPQ(list(), list(1, 2, 3), list(1, 2, 3), atusP, namesP, atusQ, namesQ);

      var allTupsAnon = atusP.Concat(atusQ);

      Assert.AreEqual("{a:1, ab:[1, 2, 3], b:'λ0'}, {a:2, ab:'λ0', b:'λ1'}, {a:3, ab:'λ1', b:'λ2'}, {a:[], b:'λ2'}", nietBruijnString(allTupsAnon));
    }

    [TestMethod]
    public void Append()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[4,5,6], as:[1,2,3], b:[1,2,3,4,5,6]}}";
      assertFirstResultFor(typeStr, atusStr, foldr(cons, id), "append");
    }
    [TestMethod]
    public void AppendToUnit()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[4], as:[1,2,3], b:[1,2,3,4]}}";
      assertFirstResultFor(typeStr, atusStr, foldr(cons, id), "append");
    }

    [TestMethod]
    public void AppendToEmpty()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[], as:[1,2,3], b:[1,2,3]}}";
      assertFirstResultFor(typeStr, atusStr, foldr(cons, id), "append");
    }

    [TestMethod]
    public void List_identity()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[], as:[1,2,3], b:[1,2,3]}}";
      assertFirstResultFor(typeStr, atusStr, foldr(cons, id), "list_id");
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
      assertNoResultFor(typeStr, atusStr);
    }

    [TestMethod]
    public void Identity_Fails_If_Initial_Is_Wrong_2()
    {
      string typeStr = "{a0:in, as:in, b:out}";
      string atusStr = "{{a0:[], as:[1,2,3], b:[1,2,3,4]}}";
      assertNoResultFor(typeStr, atusStr);
    }

    [TestMethod]
    public void Identity_Fails_If_Initial_Is_Wrong_3()
    {
      string typeStr = "{a0:in, as:in, b:out}";
      string atusStr = "{{a0:[], as:[1], b:[1,2]}}";
      assertNoResultFor(typeStr, atusStr);
    }

    [TestMethod]
    public void Identity_Fails_If_Initial_Is_Wrong_4()
    {
      string typeStr = "{a0:in, as:in, b:out}";
      string atusStr = "{{a0:[], as:[], b:[1]}}";
      assertNoResultFor(typeStr, atusStr);
    }

    [TestMethod]
    public void AppendFail_1()
    {
      string typeStr = "{a0:in, as:in, b:out}";
      string atusStr = "{{a0:[4,5,6], as:[1,2,3], b:[1,2,3]}}";
      assertNoResultFor(typeStr, atusStr);
    }
  }
}
