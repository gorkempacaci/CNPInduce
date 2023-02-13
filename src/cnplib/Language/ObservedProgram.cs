using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{

  /// <summary>
  /// An unbound program variable, an observation. Immutable object.
  /// </summary>
  public class ObservedProgram : IProgram
  {
    [Flags] public enum Constraint : short { None = 0, NotProjection = 1, NotAnd = 2}

    public readonly AlphaRelation Observables;
    public readonly ValenceVar Valence;
    public readonly Constraint Constraints;
    /// <summary>
    /// Sub-tree depth allowed for this hole. 0 should not exist, 1 would only match elementary predicates, higher values would match operators as well.
    /// </summary>
    public readonly int RemainingSearchDepth;

    public ObservedProgram(AlphaRelation obss, ValenceVar val, int remSearchDepth, Constraint constraints)
    {
      this.Observables = obss;
      this.Valence = val;
      this.Constraints = constraints;
      this.RemainingSearchDepth = remSearchDepth;

    }

    public void ReplaceFree(Free free, ITerm term)
    {
      Observables.ReplaceFreeInPlace(free, term);
    }

    public bool IsClosed => false;

    public string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    public IProgram Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public ObservedProgram FindLeftmostHole()
    {
      return this;
    }

    public (ObservedProgram, int) FindRootmostHole(int calleesDistanceToRoot = 0)
    {
      return (this, calleesDistanceToRoot);
    }

    public int GetHeight()
    {
      return 0;
    }

    public string GetTreeQualifier()
    {
      return "O";
    }

    /// <summary>
    /// Remaining search depth allows matching operators (depth>=2)
    /// </summary>
    /// <returns></returns>
    //public bool RSD_AllowsOperators()
    //{
    //  return _remainingSearchDepth >= 2;
    //}
  }
}
