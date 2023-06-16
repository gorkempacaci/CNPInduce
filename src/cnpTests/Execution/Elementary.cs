using System;
using System.Collections.Generic;
using CNP;
using CNP.Helper;
using System.Linq;
using CNP.Language;
using CNP.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace Tests.Execution
{
  [TestClass]
  public class Elementary : ExecutionTestBase
  {



    [DataTestMethod]
    [DataRow("{{a:1, b:0}}")]
    [DataRow("{{a:X, b:0}, {a:1, b:X}}")]
    [DataRow("{{a:X, b:Y}, {a:X, b:0}, {a:1, b:Y}}")]
    [DataRow("{{a:X, b:0}, {a:1, b:Y}, {a:X, b:Y}}")]
    public void Id_Fails(string relString)
    {
      var env = BuildExecution(id, relString, out var rel);
      Assert.IsFalse(env.Run(rel).IsSuccess, "run should return false");
    }

    [TestMethod]
    public void Id_Succeeds()
    {
      var env = BuildExecution(id, "{{a:H, b:H}, {a:'Hello', b:F}, {a:F, b:G}, {a:H, b:G}}", out var args);
      Assert.IsTrue(env.Run(args).IsSuccess, "run should return true");
      foreach (var tuple in args.Tuples)
      {
        Assert.IsTrue(tuple[0] is ConstantTerm ct && ct.Value is "Hello", "first term should be hello");
        Assert.IsTrue(tuple[1] is ConstantTerm ct2 && ct2.Value is "Hello", "second term should be hello");
      }
    }


    [DataTestMethod]
    [DataRow("{{a:0, b:[], ab:X}}", "{{a:0, b:[], ab:[0]}}")]
    [DataRow("{{a:0, b:[1,2], ab:X}}", "{{a:0, b:[1, 2], ab:[0, 1, 2]}}")]
    [DataRow("{{a:X, b:Y, ab:[1,2,3]}}", "{{a:1, b:[2, 3], ab:[1, 2, 3]}}")]
    [DataRow("{{a:X, b:[2|U], ab:[1,2,3]}}", "{{a:1, b:[2, 3], ab:[1, 2, 3]}}")]
    public void Cons_Succeeds(string tupBeforeRun, string tupAfterRun)
    {
      var env = BuildExecution(cons, tupBeforeRun, out var rel);
      Assert.IsTrue(env.Run(rel).IsSuccess, "cons run should return true");
      PrettyStringer ps = new PrettyStringer(env.NameBindings);
      string tupAfterRunActual = rel.Accept(ps);
      Assert.AreEqual(tupAfterRun, tupAfterRunActual);
    }

    [DataTestMethod]
    [DataRow("{{a:0, b:[1,2], ab:[0,1]}}")]
    [DataRow("{{a:1, b:[1], ab:[0,1]}}")]
    [DataRow("{{a:X, b:X, ab:[0,1]}}")]
    public void Cons_Fails(string tupBeforeRun)
    {
      var env = BuildExecution(cons, tupBeforeRun, out var rel);
      Assert.IsFalse(env.Run(rel).IsSuccess, "cons run should return false");
    }

  }
}


