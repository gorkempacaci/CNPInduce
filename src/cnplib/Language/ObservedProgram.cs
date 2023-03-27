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
    [Flags] public enum Constraint : short { None = 0, NotProjection = 1, NotAnd = 2,
      /// <summary>
      /// Only And, elementary and library predicates. This is useful because if proj adds an unbound argument then
      /// it introduces this constraint so the source predicate expression will always be well-formed.
      /// </summary>
      OnlyAndElemLib = 4}

    public readonly AlphaRelation Observables;
    public readonly ValenceVar Valence;
    public readonly Constraint Constraints;

    private readonly short[] _indicesOfInArgs;
    private readonly short[] _indicesOfOutArgs;
    
    /// <summary>
    /// Sub-tree depth allowed for this hole. 0 should not exist, 1 would only match elementary predicates, higher values would match operators as well.
    /// </summary>
    public readonly int RemainingSearchDepth;

    /// <summary>
    /// The valence that lead to this program.
    /// </summary>
    public string DebugValenceString { get; set; }
    /// <summary>
    /// The observations that lead to this program.
    /// </summary>
    public string DebugObservationString { get; set; }

    public ObservedProgram(AlphaRelation obss, ValenceVar val, int remSearchDepth, Constraint constraints)
    {
      this.Observables = obss;
      this.Valence = val;
      this.Constraints = constraints;
      this.RemainingSearchDepth = remSearchDepth;
      (_indicesOfInArgs, _indicesOfOutArgs) = val.GetIndicesOfInsOrOutsIn(obss.Names);
    }



    public void ReplaceFree(Free free, ITerm term)
    {
      Observables.ReplaceFreeInPlace(free, term);
    }

    public bool IsClosed => false;

    public string Accept(ICNPVisitor ps)
    {
      return ps.Visit(this);
    }

    public IProgram Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public ObservedProgram FindLeftmostHole()
    {
      return this;
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
    /// Returns true if all arguments where Mode is IN are ground terms in the first tuple of this observation. Doesn't check the remaining tuples because they might becomes ground as the first one is unified.
    /// </summary>
    public bool IsAllINArgumentsGroundForFirstTuple()
    {
      var firstTuple = Observables.Tuples[0];
      for (int i = 0; i < _indicesOfInArgs.Length; i++)
      {
        if (!firstTuple[_indicesOfInArgs[i]].IsGround())
          return false;
      }
      return true;
    }

    /// <summary>
    /// Returns true if all out arguments of all tuples are ground.
    /// </summary>
    public bool IsAllOutArgumentsGround()
    {
      for(int i=0; i<Observables.TuplesCount; i++)
      {
        var tuple = Observables.Tuples[i];
        for(int oii=0; oii<_indicesOfOutArgs.Length; oii++)
          if (!tuple[_indicesOfOutArgs[oii]].IsGround())
            return false;
      }

      return true;
    }
  
  }
}
