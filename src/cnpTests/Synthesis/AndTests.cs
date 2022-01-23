using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;
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
      NameVarDictionary names = new NameVarDictionary();
      Valence v = Parser.ParseNameModeMap("{a:in}", names);
      IEnumerable<AlphaTuple> atus = Parser.ParseAlphaTupleSet("{{a:1}, {a:2}}", names);
      ObservedProgram obs = ObservedProgram.CreateInitial(atus, v, 2);
      var programsInNextStep = And.CreateAtFirstHole(obs);
      Assert.AreEqual(4, programsInNextStep.Count(), "4 alternations");
      foreach(Program p in programsInNextStep)
      {
        if (p is And andProg)
        {
          if (andProg.LHOperand is ObservedProgram lhObs)
          {
            if (andProg.RHOperand is ObservedProgram rhObs)
            {
              bool LRequal = Enumerable.SequenceEqualPos(lhObs.Observables, rhObs.Observables, out int whereNot);
              Assert.IsTrue(LRequal, "LH and RH observations should be equal.");
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
      NameVarDictionary names = new NameVarDictionary();
      Valence v = Parser.ParseNameModeMap("{a:in, b:out}", names);
      IEnumerable<AlphaTuple> atus = Parser.ParseAlphaTupleSet("{{a:1, b:2}, {a:2, b:4}}", names);
      ObservedProgram obs = ObservedProgram.CreateInitial(atus, v, 2);
      var programsInNextStep = And.CreateAtFirstHole(obs);
      Assert.AreEqual(24, programsInNextStep.Count(), "24 alternations");
      var firstExpectedLH = Parser.ParseAlphaTuple("{a:1}",new());
      var firstExpectedRH = Parser.ParseAlphaTuple("{a:1, b:2}", new());
      var firstActual = programsInNextStep.First() as And;
      var firstActualLH = firstActual.LHOperand as ObservedProgram;
      var firstActualRH = firstActual.RHOperand as ObservedProgram;
      Assert.AreEqual(firstExpectedLH.ToString(), firstActualLH.Observables.First().ToString(), "First valence, LH first tuple");
      Assert.AreEqual(firstExpectedRH.ToString(), firstActualRH.Observables.First().ToString(), "First valence, RH first tuple");
      var lastExpectedLH = Parser.ParseAlphaTuple("{a:1,b:2}", new());
      var lastExpectedRH = Parser.ParseAlphaTuple("{a:1,b:2}", new());
      var lastActual = programsInNextStep.Last() as And;
      var lastActualLH = lastActual.LHOperand as ObservedProgram;
      var lastActualRH = lastActual.RHOperand as ObservedProgram;
      Assert.AreEqual(lastExpectedLH.ToString(), lastActualLH.Observables.First().ToString(), "Last valence, LH first tuple");
      Assert.AreEqual(lastExpectedRH.ToString(), lastActualRH.Observables.First().ToString(), "Last valence, RH first tuple");
    }

    [TestMethod]
    public void And_Cons_Id()
    {
      var type = "{a:in, b:out, ab:out}";
      var atus = "{{a:[], b:[], ab:[[]]}, {a:1, b:1, ab:[1|1]}}";
      assertFirstResultFor(type, atus, and(id, cons), "and(id,cons)");
    }

  }
}
