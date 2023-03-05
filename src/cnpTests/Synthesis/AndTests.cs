using System;
using System.Collections.Generic;
using CNP;
using CNP.Helper;
using System.Linq;
using CNP.Language;
using CNP.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Synthesis
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
      ObservedProgram obs = new ObservedProgram(rel, v, 2, ObservedProgram.Constraint.None);
      ProgramEnvironment env = new ProgramEnvironment(obs, names, frees);
      var programsInNextStep = And.CreateAtFirstHole(env);
      Assert.AreEqual(4, programsInNextStep.Count(), "4 alternations");
      foreach(ProgramEnvironment p in programsInNextStep)
      {
        if (p.Root is And andProg)
        {
          if (andProg.LHOperand is ObservedProgram lhObs)
          {
            if (andProg.RHOperand is ObservedProgram rhObs)
            {
              Assert.AreEqual(lhObs.Observables.TuplesCount, rhObs.Observables.TuplesCount, "LH and RH tuple counts should be equal.");
              for (int ri=0; ri<lhObs.Observables.Tuples.Length; ri++)
              {
                bool LRequal = Iterators.SequenceEqualPos(lhObs.Observables.Tuples[ri], rhObs.Observables.Tuples[ri], out int whereNot);
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
      ObservedProgram obs = new ObservedProgram(rel, v, 2, ObservedProgram.Constraint.None);
      ProgramEnvironment env = new ProgramEnvironment(obs, names, frees);
      PrettyStringer expectedStringer = new PrettyStringer(names);
      var programsInNextStep = And.CreateAtFirstHole(env);
      
      Assert.AreEqual(24, programsInNextStep.Count(), "24 alternations");
      var firstExpectedLH = Parser.ParseAlphaTupleSet("{{a:1, b:2}, {a:2, b:4}}", names, frees);
      var firstExpectedRH = Parser.ParseAlphaTupleSet("{{b:2}, {b:4}}", names, frees);
      var firstActualEnv = programsInNextStep.First();
      var firstActual = (And)firstActualEnv.Root;
      var firstActualStringer = new PrettyStringer(firstActualEnv.NameBindings);
      var firstActualLH = firstActual.LHOperand as ObservedProgram;
      var firstActualRH = firstActual.RHOperand as ObservedProgram;
      Assert.AreEqual(firstExpectedLH.Pretty(expectedStringer), firstActualLH.Observables.Pretty(firstActualStringer), "First valence, LH");
      Assert.AreEqual(firstExpectedRH.Pretty(expectedStringer), firstActualRH.Observables.Pretty(firstActualStringer), "First valence, RH ");
      var lastExpectedLH = Parser.ParseAlphaTupleSet("{{a:1,b:2}, {a:2, b:4}}", names, frees);
      var lastExpectedRH = Parser.ParseAlphaTupleSet("{{a:1,b:2}, {a:2, b:4}}", names, frees);
      var lastActualEnv = programsInNextStep.Last();
      var lastActual = (And)lastActualEnv.Root;
      var lastActualStringer = new PrettyStringer(lastActualEnv.NameBindings);
      var lastActualLH = lastActual.LHOperand as ObservedProgram;
      var lastActualRH = lastActual.RHOperand as ObservedProgram;
      Assert.AreEqual(lastExpectedLH.Pretty(expectedStringer), lastActualLH.Observables.Pretty(lastActualStringer), "Last valence LH tuples");
      Assert.AreEqual(lastExpectedRH.Pretty(expectedStringer), lastActualRH.Observables.Pretty(lastActualStringer), "Last valence RH tuples");
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
