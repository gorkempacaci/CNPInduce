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
    public readonly Valence Valence;
    /// <summary>
    /// Decompositions-To-Live. Decreased until 0 and if it's zero then that observation
    /// can only be replaced with an elementary predicate, because it doesn't afford any more
    /// height to the program tree. It's used to limit the search to a maximum depth.
    /// For example, if an OP with DTL=3 is replaced with a foldr(op1, op2), DTLs for op1 and op2 should be 2. If this value is not decreased then the search may not terminate.
    /// </summary>
    public readonly int DTL;

    public ObservedProgram(IEnumerable<AlphaTuple> obsv, Valence vlnc, int dtl) : base(false)
    {
      Observables = obsv;
      Valence = vlnc;
      DTL = dtl;
#if DEBUG // check that the valence matches the domains on the tuples
      foreach(var o in Observables)
      {
        if (!o.DomainNames.OrderBy(d=>d.Name).SequenceEqual(Valence.Keys.OrderBy(d=>d.Name), ReferenceEqualityComparer.Instance))
          throw new ArgumentException("Observation: domain names and tuple domain names don't match. ");
      }
#endif
    }

    internal override Program Clone(TermReferenceDictionary plannedParenthood)
    {
      List<AlphaTuple> clonedObservables = new();
      foreach(AlphaTuple at in Observables)
      {
        AlphaTuple net = at.Clone(plannedParenthood);
        clonedObservables.Add(net);
      }
      Valence clonedDomains = Valence.Clone(plannedParenthood);
      //var clonedObservables = Observables.Select(o => o.Clone(plannedParenthood));
      //var clonedDomains = Valence.Clone(plannedParenthood);
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
      return this.Valence.ToString() + "#" + Observables.Count() + "/TL=" + DTL;
    }
  }
}
