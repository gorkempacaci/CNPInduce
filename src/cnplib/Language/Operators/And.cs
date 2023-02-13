using System;
using System.Collections.Generic;
using CNP.Helper;
using System.Linq;

namespace CNP.Language
{
  public class And : IProgram
  {
    public readonly IProgram LHOperand, RHOperand;

    public And(IProgram lhOperand, IProgram rhOperand) 
    {
      LHOperand = lhOperand;
      RHOperand = rhOperand;
    }

    public void ReplaceFree(Free free, ITerm term)
    {
      LHOperand.ReplaceFree(free, term);
      RHOperand.ReplaceFree(free, term);
    }

    public bool IsClosed => LHOperand.IsClosed && RHOperand.IsClosed;

    public string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    public IProgram Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public string GetTreeQualifier()
    {
      return "and(" + LHOperand.GetTreeQualifier() + "," + RHOperand.GetTreeQualifier() + ")";
    }

    public int GetHeight()
    {
      return Math.Max(LHOperand.GetHeight(), RHOperand.GetHeight()) + 1;
    }

    public ObservedProgram FindLeftmostHole()
    {
      return LHOperand.FindLeftmostHole() ?? RHOperand.FindLeftmostHole();
    }

    public (ObservedProgram, int) FindRootmostHole(int calleesDistanceToRoot = 0)
    {
      var lh = LHOperand.FindRootmostHole(calleesDistanceToRoot + 1);
      var rh = RHOperand.FindRootmostHole(calleesDistanceToRoot + 1);
      if (lh.Item2 <= rh.Item2) return lh; else return rh;
    }

    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment origEnv)
    {
      var origObservation = origEnv.Root.FindHole();
      if (origObservation.RemainingSearchDepth<2)
        return Array.Empty<ProgramEnvironment>();
      if ((origObservation.Constraints & ObservedProgram.Constraint.NotAnd) == ObservedProgram.Constraint.NotAnd)
        return Array.Empty<ProgramEnvironment>();
      IEnumerable<AndValence> allValenceCombs = AndValence.Generate(origObservation.Valence);
      if (!allValenceCombs.Any())
        return Array.Empty<ProgramEnvironment>();
      List<ProgramEnvironment> programs = new List<ProgramEnvironment>();
      foreach(AndValence valComb in allValenceCombs)
      {
        //NameVar.AddNameConstraintsInPairsIfNeeded(valComb.Names);
        var pNames = valComb.LHValence.Ins.Concat(valComb.LHValence.Outs).ToArray();
        var pObs = origObservation.Observables.GetCropped(pNames);
        var pProg = new ObservedProgram(pObs, valComb.LHValence, origObservation.RemainingSearchDepth-1, ObservedProgram.Constraint.NotAnd);

        var qNames = valComb.RHValence.Ins.Concat(valComb.RHValence.Outs).ToArray();
        var qObs = origObservation.Observables.GetCropped(qNames);
        var qProg = new ObservedProgram(qObs, valComb.RHValence, origObservation.RemainingSearchDepth-1, ObservedProgram.Constraint.NotAnd);

        var andProg = new And(pProg, qProg);

        var program = origEnv.Clone((origObservation, andProg), (valComb.OnlyLHNames,valComb.OnlyRHNames));
        programs.Add(program);
      }
      return programs;
    }
  }
}
