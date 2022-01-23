using System.Collections.Generic;
using CNP.Helper.EagerLinq;
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
      var atusP = new List<AlphaTuple>();
      var atusQ = new List<AlphaTuple>();
      var namesP = new NameVarDictionary();
      var namesQ = new NameVarDictionary();
      FoldL.unfoldFoldlToPQ(list(), list(1, 2, 3), list(3, 2, 1), atusP, namesP, atusQ, namesQ);

      var allAtus = nietBruijn(atusP.Concat(atusQ));

      Assert.AreEqual("{a:1, ab:'λ0', b:[]}, {a:2, ab:'λ1', b:'λ0'}, {a:3, ab:'λ2', b:'λ1'}, {a:'λ2', b:[3, 2, 1]}", nietBruijnString(allAtus));
    }

    /*

    Expected:<{a:1, ab:'λ0', b:[]}, {a:2, ab:'λ1', b:'λ0'}, {a:3, ab:'λ2', b:'λ1'}, {a:'λ2', b:[3, 2, 1]}>.
    Actual:<(Contextless) {a:1, ab:'λ0', b:[]}, (Contextless) {a:2, ab:'λ1', b:'λ0'}, (Contextless) {a:3, ab:'λ2', b:'λ1'}, (Contextless) {a:'λ2', b:[3, 2, 1]}>. "
    */

    [TestMethod]
    public void Reverse3()
    {
      string typeStr = "{b0:in, as:in, b:out}";
      string atusStr = "{{b0:[], as:[1,2,3], b:[3,2,1]}}";
      assertFirstResultFor(typeStr, atusStr, foldl(cons, id), "reverse3");
    }


  }
}
