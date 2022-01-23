using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
  public abstract class LogicOperator : Program
  {
    public readonly Program LHOperand, RHOperand;

    protected LogicOperator(Program lhOperand, Program rhOperand) : base(lhOperand.IsClosed && rhOperand.IsClosed)
    {
      LHOperand = lhOperand;
      RHOperand = rhOperand;
    }

    internal sealed override ObservedProgram FindLeftmostHole()
    {
      return LHOperand.FindLeftmostHole() ?? RHOperand.FindLeftmostHole();
    }

    internal override (ObservedProgram, int) FindRootmostHole(int calleesDistanceToRoot = 0)
    {
      var lh = LHOperand.FindRootmostHole(calleesDistanceToRoot + 1);
      var rh = RHOperand.FindRootmostHole(calleesDistanceToRoot + 1);
      if (lh.Item2 <= rh.Item2) return lh; else return rh;
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(LHOperand, RHOperand);
    }

    public sealed override int GetHeight()
    {
      return Math.Max(LHOperand.GetHeight(), RHOperand.GetHeight()) + 1;
    }

    public override bool NameConstraintsHold()
    {
      return LHOperand.NameConstraintsHold() && RHOperand.NameConstraintsHold();
    }

    public sealed override void SetAllRootsTo(Program newRoot)
    {
      Root = newRoot;
      LHOperand.SetAllRootsTo(newRoot);
      RHOperand.SetAllRootsTo(newRoot);
    }
  }
}
