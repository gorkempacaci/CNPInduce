﻿using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language.Operators
{
  public class And : LogicOperator
  {
    public And(Program lhOperand, Program rhOperand) : base(lhOperand,rhOperand)
    {
    }

    internal override Program CloneAsSubTree(TermReferenceDictionary plannedParenthood, (ObservedProgram, Program) replaceObservation)
    {
      return new And(LHOperand.CloneAsSubTree(plannedParenthood, replaceObservation), RHOperand.CloneAsSubTree(plannedParenthood, replaceObservation));
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

    public override string ToString()
    {
      return "and(" + LHOperand.ToString() + ", " + RHOperand.ToString() + ")";
    }

    public static IEnumerable<Program> CreateAtFirstHole(Program originalProgram)
    {
      var origObservation = originalProgram.FindFirstHole();
      if (origObservation.DTL == 0)
        return Iterators.Empty<Program>();
      IEnumerable<AndOrValence> allValenceCombs = AndOrValence.Generate(origObservation.Valence);
      if (!allValenceCombs.Any())
        return Iterators.Empty<Program>();
      List<Program> programs = new List<Program>(allValenceCombs.Count());
      foreach(AndOrValence valComb in allValenceCombs)
      {
        var pObs = new ObservedProgram(origObservation.Observables, valComb.LHDoms, origObservation.DTL-1);
        var qObs = new ObservedProgram(origObservation.Observables, valComb.RHDoms, origObservation.DTL - 1);
        var andProg = new And(pObs, qObs);
        var program = originalProgram.CloneAtRoot((origObservation, andProg));
        programs.Add(program);
      }
      return programs;
    }
  }
}
