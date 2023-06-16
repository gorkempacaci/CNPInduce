using System;
using System.Collections.Generic;
using System.Collections;
using CNP.Helper;
using CNP.Helper.EagerLinq;
using System.Collections.ObjectModel;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Enumerables;

namespace CNP.Language
{

  /// <summary>
  /// A set of tuples where each position in the set of tuples is associated with a name. Like a table but column names are NameVar s.
  /// </summary>
  /// OPTIMIZE: Terms can be united into a single union struct so they're not boxed as ITerms when they lay in the AlphaRelation
  public class AlphaRelation : RelationBase
  {
    public readonly NameVar[] Names;

    /// <summary>
    /// Stores given arrays as is. Terms are given as ITerm[NumberOfTuples][NumberOfDomains]. Assumes NumberOfTuples to be non-zero. 
    /// </summary>
    public AlphaRelation(NameVar[] _names, ITerm[][] _tuples) : base(_tuples)
    {
      this.Names = _names;
    }

    public static AlphaRelation Empty = new AlphaRelation(Array.Empty<NameVar>(), new ITerm[][] { });

    public bool IsEmpty()
    {
      return Tuples.Length == 0 && Names.Length == 0;
    }

    public override AlphaRelation GetCroppedByIndices(short[] indices)
    {
      var (newNames, tups) = _getCroppedByIndices(this.Names, indices);
      return new AlphaRelation(newNames, tups);
    }

    public override string[] GetGroundNames(NameVarBindings nvb)
    {
      var names = nvb.GetNamesForVars(this.Names);
      return names;
    }

    public override AlphaRelation Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public string Accept(ICNPVisitor ps)
    {
      return ps.Visit(this);
    }

    public override string ToString()
    {
      return Accept(new PrettyStringer(VisitorOptions.Contextless));
    }

  }

}
