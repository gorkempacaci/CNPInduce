using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CNP.Helper;
using CNP.Helper.EagerLinq;
using Enumerable = System.Linq.Enumerable;

namespace CNP.Language
{

  /// <summary>
  /// An unbound program variable, an observation. Immutable object.
  /// </summary>
  public class ObservedProgram : Program
  {

    public readonly IEnumerable<AlphaTuple> Observables;
    public readonly Valence Domains;
    /// <summary>
    /// Decompositions-To-Live. Decreased until 0 and if it's zero then that observation
    /// can only be replaced with an elementary predicate, because it doesn't afford any more
    /// height to the program tree. It's used to limit the search to a maximum depth.
    /// For example, if an OP with DTL=3 is replaced with a foldr(op1, op2), DTLs for op1 and op2 should be 2. If this value is not decreased then the search may not terminate.
    /// </summary>
    public readonly int DTL;

    public ObservedProgram(IEnumerable<AlphaTuple> obsv, Valence doms, int dtl)
    {
      IsClosed = false;
      Observables = obsv;
      Domains = doms;
      DTL = dtl;
    }

    internal override Program Clone(TermReferenceDictionary plannedParenthood)
    {
      var clonedObservables = Observables.Select(o => o.Clone(plannedParenthood));
      var clonedDomains = Domains.Clone(plannedParenthood);
      return new ObservedProgram(clonedObservables, clonedDomains, DTL);
    }

    /// <summary>
    /// Replaces itself if it is the oldComponent.
    /// </summary>
    internal override Program CloneAndReplaceObservation(ObservedProgram oldComponent, Program newComponent, TermReferenceDictionary plannedParenthood)
    {
      // If this is the oldComponent they're looking for
      if (object.ReferenceEquals(this, oldComponent))
      {
        return newComponent;
      }
      else
      {
        return this.Clone(plannedParenthood);
      }
    }

    internal override ObservedProgram FindFirstHole()
    {
      return this;
    }
    public override int GetHeight()
    {
      return 0;
    }
    public override void SetAllRootsTo(Program newRoot)
    {
      Root = newRoot;
    }

    public override string ToString()
    {
      return this.Domains.ToString() + "/" + Observables.Count();
    }
  }
}
