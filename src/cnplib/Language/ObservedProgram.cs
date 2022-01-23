using System;
using System.Collections.Generic;
using CNP.Display;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{

  /// <summary>
  /// An unbound program variable, an observation. Immutable object.
  /// </summary>
  public class ObservedProgram : Program
  {
    [Flags] public enum Constraint : short { None = 0, NotProjection = 1, NotAnd = 2}

    public readonly IEnumerable<AlphaTuple> Observables;
    public readonly Valence Valence;
    public readonly Constraint Constraints;

    /// <summary>
    /// Sub-tree depth allowed for this hole. 0 should not exist, 1 would only match elementary predicates, higher values would match operators as well.
    /// </summary>
    internal readonly int _remainingSearchDepth;

    public override string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    public static ObservedProgram CreateInitial(IEnumerable<AlphaTuple> obsv, Valence vlnc, int searchDepth)
    {
      return new ObservedProgram(obsv, vlnc, searchDepth);
    }

    /// <summary>
    /// Parent observation is passed in so the RemainingSearchDepth of this observation is one less than the parent.
    /// </summary>
    public ObservedProgram(IEnumerable<AlphaTuple> obsv, Valence vlnc, ObservedProgram parentObservation, Constraint cnstrnts = Constraint.None) : this(obsv, vlnc, parentObservation._remainingSearchDepth-1, cnstrnts)
    {

    }

    private ObservedProgram(IEnumerable<AlphaTuple> obsv, Valence vlnc, int remainingSearchDepth, Constraint cnstrnts = Constraint.None) : base(false)
    {
      if (!obsv.Any())
        throw new InvalidOperationException("Empty observation.");
      Observables = obsv;
      Valence = vlnc;
      _remainingSearchDepth = remainingSearchDepth;
      Constraints = cnstrnts;
#if DEBUG
      if (_remainingSearchDepth <= 0)
        throw new ArgumentOutOfRangeException("Remaining search depth should not reach 0.");
      // check that the valence matches the domains on the tuples
      foreach(var o in Observables)
      {
        if (!o.DomainNames.OrderBy(n=>n.GetHashCode()).SequenceEqual(Valence.Keys.OrderBy(n=>n.GetHashCode()), ReferenceEqualityComparer.Instance))
          throw new ArgumentException("Observation: domain names and tuple domain names don't match. ");
      }
#endif
    }

    internal override Program CloneAsSubTree(TermReferenceDictionary plannedParenthood, (ObservedProgram, Program) replaceObservation = default)
    {
      // If this is the oldComponent they're looking for
      if (object.ReferenceEquals(this, replaceObservation.Item1))
      {
        return replaceObservation.Item2.CloneAsSubTree(plannedParenthood,  (null,null));
      }
      else
      {
        List<AlphaTuple> clonedObservables = new();
        foreach (AlphaTuple at in Observables)
        {
          AlphaTuple net = at.Clone(plannedParenthood);
          clonedObservables.Add(net);
        }
        Valence clonedDomains = Valence.Clone(plannedParenthood);
        return new ObservedProgram(clonedObservables, clonedDomains, _remainingSearchDepth, Constraints);
      }
    }

    internal override ObservedProgram FindLeftmostHole()
    {
      return this;
    }

    internal override (ObservedProgram, int) FindRootmostHole(int calleesDistanceToRoot = 0)
    {
      return (this, calleesDistanceToRoot);
    }

    public override int GetHeight()
    {
      return 0;
    }

    public override bool NameConstraintsHold()
    {
      return Valence.NameConstraintsHold();
    }

    public override void SetAllRootsTo(Program newRoot)
    {
      Root = newRoot;
    }

    public override string GetTreeQualifier()
    {
      return "O";
    }

    /// <summary>
    /// Remaining search depth allows matching operators (depth>=2)
    /// </summary>
    /// <returns></returns>
    public bool RSD_AllowsOperators()
    {
      return _remainingSearchDepth >= 2;
    }
  }
}
