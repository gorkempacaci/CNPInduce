using System;
using CNP.Language;
using CNP.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Execution
{
  [TestClass]
  public class And : ExecutionTestBase
  {

    /// <summary>
    /// Useful for 'sorted' relation: for each pair of (a, b), returns a if a <= b, fails otherwise.
    /// </summary>
    [DataTestMethod]
    [DataRow("{{a:4, b:5, ab:X}}", 4)]
    [DataRow("{{a:5, b:5, ab:X}}", 5)]
    public void And_MinPassIfLte_Success(string relStr, int expected)
    {
      var env = BuildExecution(and(min, leq), relStr, out var rel);
      var res = env.Run(rel);
      Assert.IsTrue(res.IsSuccess);
      Assert.AreEqual(expected, ((ConstantTerm)rel.Tuples[0][2]).Value);
    }

    /// <summary>
    /// Useful for 'sorted' relation: for each pair of (a, b), returns a if a <= b, fails otherwise.
    /// </summary>
    [DataTestMethod]
    [DataRow("{{a:4, b:5, ab:X}}", 4)]
    [DataRow("{{a:5, b:5, ab:X}}", 5)]
    public void And_MinPassIfLte_Alt_Success(string relStr, int expected)
    {
      var env = BuildExecution(and(leq, min), relStr, out var rel);
      var res = env.Run(rel);
      Assert.IsTrue(res.IsSuccess);
      Assert.AreEqual(expected, ((ConstantTerm)rel.Tuples[0][2]).Value);
    }

    /// <summary>
    /// Useful for 'sorted' relation: for each pair of (a, b), returns a if a <= b, fails otherwise.
    /// </summary>
    [DataTestMethod]
    [DataRow("{{a:5, b:4, ab:X}}")]
    public void And_MinPassIfLte_Fail(string relStr)
    {
      var env = BuildExecution(and(min, leq), relStr, out var rel);
      var res = env.Run(rel);
      Assert.IsFalse(res.IsSuccess);
    }

    /// <summary>
    /// Useful for 'sorted' relation: for each pair of (a, b), returns a if a <= b, fails otherwise.
    /// </summary>
    [DataTestMethod]
    [DataRow("{{a:5, b:4, ab:X}}")]
    public void And_MinPassIfLte_Alt_Fail(string relStr)
    {
      var env = BuildExecution(and(leq, min), relStr, out var rel);
      var res = env.Run(rel);
      Assert.IsFalse(res.IsSuccess);
    }
  }
}

