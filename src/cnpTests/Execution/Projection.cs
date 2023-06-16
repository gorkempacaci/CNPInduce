using System;
using CNP.Language;
using CNP.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Execution
{
  [TestClass]
  public class Projection : ExecutionTestBase
  {
    /// <summary>
    /// Useful for 'sorted' relation: for each pair of (a, b), returns a if a <= b, fails otherwise.
    /// </summary>
    [DataTestMethod]
    [DataRow("{{list:[1,2,3], head:X}}", 1)]
    [DataRow("{{list:[1], head:X}}", 1)]
    public void Success_head_by_cons(string tuple, int expected)
    {
      NameVarBindings names = new NameVarBindings();
      FreeFactory frees = new FreeFactory();
      var (nameList, nameHead) = names.AddNameVars("list", "head");
      var (consA, consB, consAB) = names.AddNameVars("a", "b", "ab");
      var program = proj(cons, new[] { (consAB, nameList), (consA, nameHead) });
      var args = Parser.ParseGroundRelation(tuple, frees);
      ExecutionEnvironment ee = new ExecutionEnvironment(program, names, frees);
      var res = ee.Run(args);
      Assert.IsTrue(res.IsSuccess);
      Assert.AreEqual(expected, ((ConstantTerm)args.Tuples[0][1]).Value);
    }

    /// <summary>
    /// Useful for 'sorted' relation: for each pair of (a, b), returns a if a <= b, fails otherwise.
    /// </summary>
    [DataTestMethod]
    [DataRow("{{list:[1,2,3], head:2}}", 2)]
    [DataRow("{{list:[], head:X}}", null)]
    public void Fail_head_by_cons(string tuple, int? expected)
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
      if (expected is not null)
        Assert.AreEqual(expected, ((ConstantTerm)args.Tuples[0][1]).Value);
    }

    [TestMethod]
    public void Fail_For_sorted()
    {
      NameVarBindings nvb = new();
      NameVar b0_inner = nvb.AddNameVar("b0");
      NameVar b0_outer = nvb.AddNameVar("b0");
      NameVar as_inner = nvb.AddNameVar("as");
      NameVar as_outer = nvb.AddNameVar("as");
      ConstantTerm zero = constterm(0);
      FreeFactory frees = new();
      IProgram prog =
        and(constant(b0_outer, constterm(0)),
            proj(and(constant(b0_inner, zero),
                     foldl(plus)),
                 new[] { (b0_inner, b0_outer), (as_inner, as_outer) }));
      GroundRelation args = Parser.ParseGroundRelation("{{b0:0, as:[2,1,3]}}", frees);
      ExecutionEnvironment exenv = new ExecutionEnvironment(prog, nvb, frees);
      var result = exenv.Run(args);
      Assert.IsFalse(result.IsSuccess);
    }
  }
}

