using System;
using System.Collections.Generic;
using CNP.Display;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
  public class And : LogicOperator
  {
    public And(Program lhOperand, Program rhOperand) : base(lhOperand,rhOperand)
    {
    }

    internal override Program CloneAsSubTree(TermReferenceDictionary plannedParenthood, (ObservedProgram, Program) replaceObservation)
    {
      var lh = LHOperand.CloneAsSubTree(plannedParenthood, replaceObservation);
      var rh = RHOperand.CloneAsSubTree(plannedParenthood, replaceObservation);
      return new And(lh, rh);
    }

    public override bool Equals(object obj)
    {
      return obj is And otherAnd &&
        LHOperand.Equals(otherAnd.LHOperand) &&
        RHOperand.Equals(otherAnd.RHOperand);
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    public static IEnumerable<Program> CreateAtFirstHole(Program originalProgram)
    {
      var origObservation = originalProgram.FindHole();
      if (!origObservation.RSD_AllowsOperators())
        return Iterators.Empty<Program>();
      if ((origObservation.Constraints & ObservedProgram.Constraint.NotAnd) == ObservedProgram.Constraint.NotAnd)
        return Iterators.Empty<Program>();
      IEnumerable<AndOrValence> allValenceCombs = AndOrValence.Generate(origObservation.Valence);
      if (!allValenceCombs.Any())
        return Iterators.Empty<Program>();
      List<Program> programs = new List<Program>();
      foreach(AndOrValence valComb in allValenceCombs)
      {
        //NameVar.AddNameConstraintsInPairsIfNeeded(valComb.Names);
        var pObs = origObservation.Observables.Select(atu => atu.Crop(valComb.LHDoms.Keys));
        var pProg = new ObservedProgram(pObs, valComb.LHDoms, origObservation, ObservedProgram.Constraint.NotAnd);
        var qObs = origObservation.Observables.Select(atu => atu.Crop(valComb.RHDoms.Keys));
        var qProg = new ObservedProgram(qObs, valComb.RHDoms, origObservation, ObservedProgram.Constraint.NotAnd);
        var andProg = new And(pProg, qProg);
        var program = originalProgram.CloneAtRoot((origObservation, andProg));
        programs.Add(program);
      }
#if DEBUG
        int c = programs.Count();
        Debugging.LogObjectWithMax("and", programs.Count(), programs);
      //if (c > 1000)
      //  throw new ArgumentOutOfRangeException();
#endif
      return programs;
    }

    public override string GetTreeQualifier()
    {
      return "and(" + LHOperand.GetTreeQualifier() + "," + RHOperand.GetTreeQualifier() + ")";
    }
  }
}
