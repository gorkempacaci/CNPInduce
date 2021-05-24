using System;
using CNP.Helper;

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
  }
}
