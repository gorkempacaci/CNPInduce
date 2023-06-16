using System;
using System.Linq;
using CNP.Language;
using CNP.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Execution
{
  [TestClass]
  public class Folds : ExecutionTestBase
  {
    /// <summary>
    /// Useful for 'sorted' relation: for each pair of (a, b), returns a if a <= b, fails otherwise.
    /// </summary>
    [DataTestMethod]
    [DataRow("{{as:[1,2,3], b0:[], b:X}}", new[] {3,2,1})]
    [DataRow("{{as:[1], b0:[], b:X}}", new[] {1})]
    public void Success_foldl_cons(string relStr, int[] expected)
    {
      var env = BuildExecution(foldl(cons), relStr, out var rel);
      var res = env.Run(rel);
      Assert.IsTrue(res.IsSuccess);
      var bTerm = (TermList)rel.Tuples[0][2];
      var elems = bTerm.ToEnumerable().Select(t => (int)((ConstantTerm)t).Value).ToArray();
      CollectionAssert.AreEqual(expected, elems);
    }

    /// <summary>
    /// Useful for 'sorted' relation: for each pair of (a, b), returns a if a <= b, fails otherwise.
    /// </summary>
    [DataTestMethod]
    [DataRow("{{as:[1,2,3], b0:[], b:[]}}")]
    [DataRow("{{as:[1,2,3], b0:[], b:[1,2,3|X]}}")]
    public void Fail_fold_cons(string tuple)
    {
      NameVarBindings names = new NameVarBindings();
      FreeFactory frees = new FreeFactory();
      var (nameList, nameHead) = names.AddNameVars("list", "head");
      var (consA, consB, consAB) = names.AddNameVars("a", "b", "ab");
      var program = proj(cons, new[] { (consAB, nameList), (consA, nameHead) });
      var args = Parser.ParseGroundRelation(tuple, frees);
      ExecutionEnvironment ee = new ExecutionEnvironment(program, names, frees);
      var res = ee.Run(args);
      Assert.IsFalse(res.IsSuccess);
    }
  }
}

