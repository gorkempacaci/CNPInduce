using System;
using System.Collections.Generic;
using CNP;
using CNP.Helper;
using System.Linq;
using CNP.Language;
using CNP.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Synthesis
{
  [TestClass]
  public class AndTests : TestBase
  {
    [TestMethod]
    public void And_FillFirstHole_1in()
    {
      NameVarBindings names = new();
      FreeFactory frees = new();
      ValenceVar v = Parser.ParseValence("{a:in}", names);
      AlphaRelation rel = Parser.ParseAlphaTupleSet("{{a:1}, {a:2}}", names, frees);
      ObservedProgram obs = new ObservedProgram(rel, v, 2, 0, ObservedProgram.Constraint.None);
      ProgramEnvironment env = new ProgramEnvironment(obs, names, frees);
      var programsInNextStep = And.CreateAtFirstHole(env).ToArray();
      Assert.AreEqual(2, programsInNextStep.Count(), "2 alternations");
      var obsCount = programsInNextStep.Sum(p => ((p.Root as And).RHOperand as ObservedProgram).Observations.Length);
      Assert.AreEqual(4, obsCount, "4 total observations");

      foreach (ProgramEnvironment p in programsInNextStep)
      {
        if (p.Root is And andProg)
        {
          if (andProg.LHOperand is ObservedProgram lhObs)
          {
            if (andProg.RHOperand is ObservedProgram rhObs)
            {
              Assert.AreEqual(lhObs.Observations[0].Examples.TuplesCount, rhObs.Observations[0].Examples.TuplesCount, "LH and RH tuple counts should be equal.");
              for (int ri=0; ri < lhObs.Observations[0].Examples.Tuples.Length; ri++)
              {
                bool LRequal = Iterators.SequenceEqualPos(lhObs.Observations[0].Examples.Tuples[ri], rhObs.Observations[0].Examples.Tuples[ri], out int whereNot);
                Assert.IsTrue(LRequal, "LH and RH observations should be equal.");
              }
            }
            else Assert.Fail("RH of And should be observation");
          }
          else Assert.Fail("LH of And should be observation");
        }
        else Assert.Fail("Alternate programs should be and programs");
      }
    }

    [TestMethod]
    public void And_FillFirstHole_1in_1out()
    {
      NameVarBindings names = new();
      FreeFactory frees = new FreeFactory();
      FreeDictionary freeDict = new FreeDictionary(frees);
      ValenceVar v = Parser.ParseValence("{a:in, b:out}", names);
      AlphaRelation rel = Parser.ParseAlphaTupleSet("{{a:1, b:2}, {a:2, b:4}}", names, frees);
      ObservedProgram obs = new ObservedProgram(rel, v, 2, 0, ObservedProgram.Constraint.None);
      ProgramEnvironment env = new ProgramEnvironment(obs, names, frees);
      PrettyStringer expectedStringer = new PrettyStringer(names);
      var programsInNextStep = And.CreateAtFirstHole(env);

      Assert.AreEqual(5, programsInNextStep.Count(), "2 alternations");
      var obsCount = programsInNextStep.Sum(p => ((p.Root as And).RHOperand as ObservedProgram).Observations.Length);
      Assert.AreEqual(24, obsCount, "4 total observations");

      var firstExpectedLH = Parser.ParseAlphaTupleSet("{{b:2}, {b:4}}", names, frees);
      var firstExpectedRH = Parser.ParseAlphaTupleSet("{{a:1, b:2}, {a:2, b:4}}", names, frees);
      var firstActualEnv = programsInNextStep.First();
      var firstActual = (And)firstActualEnv.Root;
      var firstActualStringer = new PrettyStringer(firstActualEnv.NameBindings);
      var firstActualLH = firstActual.LHOperand as ObservedProgram;
      var firstActualRH = firstActual.RHOperand as ObservedProgram;
      Assert.AreEqual(firstExpectedLH.Accept(expectedStringer), firstActualLH.Observations[0].Examples.Accept(firstActualStringer), "First valence, LH");
      Assert.AreEqual(firstExpectedRH.Accept(expectedStringer), firstActualRH.Observations[0].Examples.Accept(firstActualStringer), "First valence, RH ");
      var lastExpectedLH = Parser.ParseAlphaTupleSet("{{a:1,b:2}, {a:2, b:4}}", names, frees);
      var lastExpectedRH = Parser.ParseAlphaTupleSet("{{b:2}, {b:4}}", names, frees);
      var lastActualEnv = programsInNextStep.Last();
      var lastActual = (And)lastActualEnv.Root;
      var lastActualStringer = new PrettyStringer(lastActualEnv.NameBindings);
      var lastActualLH = lastActual.LHOperand as ObservedProgram;
      var lastActualRH = lastActual.RHOperand as ObservedProgram;
      Assert.AreEqual(lastExpectedLH.Accept(expectedStringer), lastActualLH.Observations[0].Examples.Accept(lastActualStringer), "Last valence LH tuples");
      Assert.AreEqual(lastExpectedRH.Accept(expectedStringer), lastActualRH.Observations[0].Examples.Accept(lastActualStringer), "Last valence RH tuples");
    }

    [TestMethod]
    public void And_Cons_Id()
    {
      var type = "{a:in, b:out, ab:out}";
      var atus = "{{a:[], b:[], ab:[[]]}, {a:1, b:1, ab:[1|1]}}";
      assertFirstResultFor(type, atus, "and(id, cons)");
    }

  }
}
