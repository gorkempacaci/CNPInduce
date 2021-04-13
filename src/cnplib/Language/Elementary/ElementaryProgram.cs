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

    protected sealed override Program CloneAndReplaceObservationAtNode(ObservedProgram oldComponent, Program newComponent, TermReferenceDictionary plannedParenthood)
    {
      return CloneNode(plannedParenthood);
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
