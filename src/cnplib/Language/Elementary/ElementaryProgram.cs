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

    internal override ObservedProgram FindFirstHole()
    {
      return null;
    }

    public override int GetHeight()
    {
      return 0;
    }

    public sealed override void SetAllRootsTo(Program newRoot)
    {
      Root = newRoot;
    }
  }
}
