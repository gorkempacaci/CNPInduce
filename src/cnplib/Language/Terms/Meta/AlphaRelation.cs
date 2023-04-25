using System;
using System.Collections.Generic;
using System.Collections;
using CNP.Helper;
using CNP.Helper.EagerLinq;
using System.Collections.ObjectModel;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Enumerables;
using System.Linq;

namespace CNP.Language
{
  /// <summary>
  /// A set of tuples where each position in the set of tuples is associated with a name. Like a table but column names are NameVar s.
  /// </summary>
  /// OPTIMIZE: Terms can be united into a single union struct so they're not boxed as ITerms when they lay in the AlphaRelation
  public class AlphaRelation
  {
    public readonly NameVar[] Names;
    /// <summary>
    /// ITerm[NumberOfTuples, NumberOfDomains]
    /// </summary>
    public readonly ITerm[][] Tuples;

    public readonly int TuplesCount;
    public readonly int ColumnsCount;
    /// <summary>
    /// Would be equivalent to rows * counts
    /// </summary>
    public readonly int CountOfElements;

    /// <summary>
    /// Stores given arrays as is. Terms are given as ITerm[NumberOfTuples][NumberOfDomains]. Assumes NumberOfTuples to be non-zero. 
    /// </summary>
    public AlphaRelation(NameVar[] _names, ITerm[][] _tuples)
    {
      this.Names = _names;
      this.Tuples = _tuples;
      this.TuplesCount = this.Tuples.Length;
      this.ColumnsCount = this.Tuples[0].Length;
      this.CountOfElements = TuplesCount * ColumnsCount;
    }

    /// <summary>
    /// Crops columns of this relation to the colums given by protoValence's LH and RH parts. ProtoAndValence's modes need to be in the same order as this alpharelation's names.
    /// </summary>
    public (ITerm[][] lhTerms, ITerm[][] rhTerms) GetCroppedTo2ByIndices(short[] lhIndices, short[] rhIndices)
    {
      var lhTuples = new ITerm[Tuples.Length][];
      var rhTuples = new ITerm[Tuples.Length][];
      for(int ti=0; ti<Tuples.Length; ti++)
      {
        lhTuples[ti] = new ITerm[lhIndices.Length];
        for (int lti = 0; lti < lhIndices.Length; lti++)
          lhTuples[ti][lti] = Tuples[ti][lhIndices[lti]];
        rhTuples[ti] = new ITerm[rhIndices.Length];
        for (int rti = 0; rti < rhIndices.Length; rti++)
          rhTuples[ti][rti] = Tuples[ti][rhIndices[rti]];
      }
      return (lhTuples, rhTuples);
    }

    /// <summary>
    /// Replaces all given frees <paramref name="free"/> in this with the given term <paramref name="term"/>. Applies recursively to all terms and their subterms (through lists)
    /// </summary>
    public void ReplaceFreeInPlace(Free free, ITerm term)
    {
      for(int ri=0; ri<Tuples.Length; ri++)
      {
        for(int ci=0; ci < Tuples[ri].Length; ci++)
        {
          Tuples[ri][ci] = Tuples[ri][ci].GetFreeReplaced(free, term);
        }
      }
    }

    public AlphaRelation Clone(CloningContext cc)
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
