using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;
using CNP.Language;
using CNP.Language.Operators;
using CNP.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Synthesis
{
  [TestClass]
  public class AndTests : TestBase
  {
    [TestMethod]
    public void And_FillFirstHole()
    {
      NameVarDictionary names = new NameVarDictionary();
      Valence v = Parser.ParseNameModeMap("{a:in}", names);
      IEnumerable<AlphaTuple> atus = Parser.ParseAlphaTupleSet("{{a:1}, {a:2}}", names);
      ObservedProgram obs = new ObservedProgram(atus, v, 2, ObservedProgram.Constraint.None);
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
    
  }
}
