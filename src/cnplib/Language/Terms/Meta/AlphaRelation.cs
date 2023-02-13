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
    /// Stores given arrays as is. Terms are given as ITerm[NumberOfTuples][NumberOfDomains]
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
    /// Returns a column-cropped copy of the set. The returning set will contain same number of tuples but less columns.
    /// </summary>
    /// OPTIMIZE: May be improved by ordering selectedDomains to the same order of Names, and then for each row picking the selected elements in one pass.
    public AlphaRelation GetCropped(NameVar[] selectedDomains)
    {
      var indices = new int[selectedDomains.Length];
      for (int s = 0; s < selectedDomains.Length; s++)
      {
        bool found = false;
        for (int i = 0; i < Names.Length; i++)
        {
          if (Names[i].Index == selectedDomains[s].Index)
          {
            indices[s] = i;
            found = true;
            break;
          }
        }
        if (!found)
          throw new ArgumentOutOfRangeException("Cannot crop because the selected domain does not exist.");
      }
      var newTuples = new ITerm[Tuples.Length][];
      for(int ti=0; ti<Tuples.Length; ti++)
      {
        newTuples[ti] = new ITerm[selectedDomains.Length];
        for (int ii=0; ii<indices.Length; ii++)
        {
          newTuples[ti][ii] = Tuples[ti][indices[ii]];
        }
      }
      return new AlphaRelation(selectedDomains, newTuples);
    }

    /// <summary>
    /// Overwrites the terms in the 'site' if necessary to unify them with terms in the unifier, in their given order. Performs substitutions through the given env, so they apply to the whole root context. Mutates the given siteRow and its environment so it needs to be disposed if the unification fails. Skips positions i where unifierRow[i] is null.
    /// </summary>
    public static bool UnifyInPlace(ITerm[] siteRow, ProgramEnvironment env, ITerm[] unifierRow)
    {
      for (int ei = 0; ei < siteRow.Length; ei++)
      {
        if (unifierRow[ei] is null)
          continue;
        while (false == ITerm.UnifyOrSuggest(siteRow[ei], unifierRow[ei], out var suggSubstitution))
        {
          if (suggSubstitution.HasValue)
          { // needs a substitution
            // for the siteRow
            env.ReplaceFree(suggSubstitution.Value.Item1, suggSubstitution.Value.Item2);
            // for unifierRow
            for (int i = 0; i < unifierRow.Length; i++)
              if (unifierRow[i] != null)
                unifierRow[i] = unifierRow[i].GetFreeReplaced(suggSubstitution.Value.Item1, suggSubstitution.Value.Item2);
          }
          else
          { // no way for unification for this tuple, therefore no way for the whole relation.
            return false;
          }
        }
      }
      return true;
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

    public string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    public override string ToString()
    {
      return Pretty(new PrettyStringer(PrettyStringer.Options.Contextless));
    }

  }

}
