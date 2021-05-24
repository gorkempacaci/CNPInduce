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

    internal sealed override ObservedProgram FindFirstHole()
    {
      return LHOperand.FindFirstHole() ?? RHOperand.FindFirstHole();
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(LHOperand, RHOperand);
    }

    public sealed override int GetHeight()
    {
      return Math.Max(LHOperand.GetHeight(), RHOperand.GetHeight()) + 1;
    }

    public sealed override void SetAllRootsTo(Program newRoot)
    {
      Root = newRoot;
      LHOperand.SetAllRootsTo(newRoot);
      RHOperand.SetAllRootsTo(newRoot);
    }
  }
}
