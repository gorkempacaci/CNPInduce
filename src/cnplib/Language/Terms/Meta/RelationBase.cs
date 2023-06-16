using System;
using System.Linq;

namespace CNP.Language
{
  public abstract class RelationBase
  {
    public abstract RelationBase Clone(CloningContext cc);
    /// <summary>
    /// ITerm[NumberOfTuples, NumberOfDomains]
    /// </summary>
    public ITerm[][] Tuples { get; init; }

    public readonly int TuplesCount;
    public readonly int ColumnsCount;
    /// <summary>
    /// Would be equivalent to rows * counts
    /// </summary>
    public readonly int CountOfElements;

    public RelationBase(ITerm[][] tups)
    {
      this.Tuples = tups;
      this.TuplesCount = this.Tuples.Length;
      if (TuplesCount>0)
        this.ColumnsCount = this.Tuples[0].Length;
      this.CountOfElements = TuplesCount * ColumnsCount;
    }

    public abstract string[] GetGroundNames(NameVarBindings nvb);
    


    public short GetNameIndex(NameVarBindings nvb, string groundName)
    {
      var names = GetGroundNames(nvb);
      return (short)Array.IndexOf(names, groundName);
    }

    public (short, short) GetNameIndices(NameVarBindings nvb, string groundName1, string groundName2)
    {
      var names = GetGroundNames(nvb);
      return ((short)Array.IndexOf(names, groundName1), (short)Array.IndexOf(names, groundName2));
    }

    public (short, short, short) GetNameIndices(NameVarBindings nvb, string groundName1, string groundName2, string groundName3)
    {
      var names = GetGroundNames(nvb);
      return ((short)Array.IndexOf(names, groundName1), (short)Array.IndexOf(names, groundName2), (short)Array.IndexOf(names, groundName3));
    }

    public short[] GetIndicesOfGroundNames(string[] names, NameVarBindings nvb)
    {
      var myGroundNames = GetGroundNames(nvb);
      var indices = names.Select(n => (short)Array.IndexOf(myGroundNames, n)).ToArray();
      return indices;
    }


    /// <summary>
    /// Replaces all given frees <paramref name="free"/> in this with the given term <paramref name="term"/>. Applies recursively to all terms and their subterms (through lists)
    /// </summary>
    public void ReplaceFreeInPlace(Free free, ITerm term)
    {
      for (int ri = 0; ri < Tuples.Length; ri++)
      {
        for (int ci = 0; ci < Tuples[ri].Length; ci++)
        {
          Tuples[ri][ci] = Tuples[ri][ci].GetFreeReplaced(free, term);
        }
      }
    }

    public abstract RelationBase GetCroppedByIndices(short[] indices);

    /// <summary>
    /// Crops columns of this relation to the colums given by protoValence's LH and RH parts. ProtoAndValence's modes need to be in the same order as this alpharelation's names.
    /// </summary>
    protected (TName[], ITerm[][]) _getCroppedByIndices<TName>(TName[] names, short[] indices)
    {
      var tuples = new ITerm[Tuples.Length][];
      var newNames = new TName[indices.Length];
      for (int ni = 0; ni < indices.Length; ni++)
      {
        newNames[ni] = names[indices[ni]];
      }
      for (int tupi = 0; tupi < Tuples.Length; tupi++)
      {
        tuples[tupi] = new ITerm[indices.Length];
        for (int termi = 0; termi < indices.Length; termi++)
          tuples[tupi][termi] = Tuples[tupi][indices[termi]];
      }
      return (newNames, tuples);
    }
  }

}
