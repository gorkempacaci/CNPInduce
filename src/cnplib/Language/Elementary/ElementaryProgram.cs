using System;
using System.Collections.Generic;
using CNP.Helper;

namespace CNP.Language
{
  public abstract class ElementaryProgram : Program
  {
    public ElementaryProgram() : base(true)
    {
      
    }

    internal override ObservedProgram FindLeftmostHole()
    {
      return null;
    }

    internal override (ObservedProgram, int) FindRootmostHole(int calleesDistanceToRoot = 0)
    {
      return (null, int.MaxValue);
    }

    public override int GetHeight()
    {
      return 0;
    }

    public override bool NameConstraintsHold()
    {
      return true;
    }

    public sealed override void SetAllRootsTo(Program newRoot)
    {
      Root = newRoot;
    }

    public override string GetTreeQualifier()
    {
      return "p";
    }
  }
}
