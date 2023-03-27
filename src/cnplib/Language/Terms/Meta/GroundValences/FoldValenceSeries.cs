using System.Collections.Generic;
using System.Linq;

namespace CNP.Language
{
  /// <summary>
  /// A series of triplets of mode arrays, where each triplet has paired modes for the fold (composition), recursive and base cases (components). Names in the 'Names' array have the same array as the Modes, for all three cases.
  /// </summary>
  /// <param name="Names"></param>
  /// <param name="RecursiveCaseNames"></param>
  /// <param name="BaseCaseNames"></param>
  /// <param name="ModeTriplesByModeNumber"></param>
  public readonly record struct FoldValenceSeries
  {
    public readonly string[] Names;
    public readonly string[] RecursiveCaseNames;
    public readonly IReadOnlyDictionary<int, FoldModeIndices[]> FoldModesByModeNumber;

    public FoldValenceSeries(string[] names, string[] recNames, IReadOnlyDictionary<int, FoldModeIndices[]> foldModesByNumber)
    {
      this.Names = names;
      this.RecursiveCaseNames = recNames;
      this.FoldModesByModeNumber = foldModesByNumber;
    }


    public static FoldValenceSeries FoldSerieFromArrays(string[] Names, string[] RecursiveCaseNames, (Mode[] FoldModes, Mode[] RecModes)[] FoldModeTriplesArray)
    {
      var foldModeTriplesIndices = FoldModeTriplesArray.Select(e => new
        FoldModeIndices(ModeIndices.IndicesFromArray(e.FoldModes),
                               ModeIndices.IndicesFromArray(e.RecModes)))
                                                       .GroupBy(e => e.GetHashCode())
                                                       .ToDictionary(g => g.Key, g => g.ToArray());
      return new FoldValenceSeries(Names, RecursiveCaseNames, foldModeTriplesIndices);
    }

    /// <summary>
    /// Returns the list of compatible valences with all their permutations assigning free names to ground names in the valence list. If the valencevar's name is ground to begin with, that name is returned as the single alternative. Does not modify the names. In the groundingAlternatives, for each alternative, modeIndices for the baseCase and recCase are also returned. The names for these are static so there is no grounding for these.
    /// </summary>
    public void GroundingAlternatives(ValenceVar vv, NameVarBindings vvbind, out List<(string[] ins, string[] outs, ModeIndices rec)> groundingAlternatives)
    {
      groundingAlternatives = new();
      if (FoldModesByModeNumber.TryGetValue(vv.ModeNumber, out FoldModeIndices[] foldValenceAlternatives))
      {
        string[] searchedInVars = vvbind.GetNamesForVars(vv.Ins);
        string[] searchedOutVars = vvbind.GetNamesForVars(vv.Outs);
        //OPTIMIZE: There may still be duplicates? If so, it might make sense to distinct the final list of grounding alternatives.
        foreach (FoldModeIndices valenceAltCombi in foldValenceAlternatives)
        {
          if (GroundValence.MatchingAlternatesForNameVars(searchedInVars, Names, valenceAltCombi.FoldModes.Ins, out var inAlternatives))
          {
            if (GroundValence.MatchingAlternatesForNameVars(searchedOutVars, Names, valenceAltCombi.FoldModes.Outs, out var outAlternatives))
            {
              foreach (var insAlt in inAlternatives)
                foreach (var outsAlt in outAlternatives)
                  groundingAlternatives.Add((insAlt, outsAlt, valenceAltCombi.RecursiveCaseModes));
            }
            else continue; // no out-unifications
          }
          else continue; // no in-unifications
        }
      }
    }
  }

}