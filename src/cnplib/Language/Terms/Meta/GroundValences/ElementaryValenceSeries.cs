using System;
using System.Collections.Generic;
using System.Linq;

namespace CNP.Language
{
  /// <summary>
  /// A series of modes all having the same names. Mode[] in the dictionary has the same order as the names in the 'Names' so their length is the same.
  /// </summary>
  public readonly record struct ElementaryValenceSeries
  {
    public readonly string[] Names;
    public readonly IReadOnlyDictionary<int, ModeIndices[]> ModesByModeNumber;

    public ElementaryValenceSeries(string[] names, IReadOnlyDictionary<int, ModeIndices[]> modesByNumber)
    {
      this.Names = names;
      this.ModesByModeNumber = modesByNumber;
    }

    public static ElementaryValenceSeries SeriesFromArrays(string[] Names, Mode[][] arrayOfModeArrays)
    {
      var modesDict = arrayOfModeArrays.Select(ms => ModeIndices.IndicesFromArray(ms))
                                       .GroupBy(msi => msi.GetHashCode())
                                       .ToDictionary(g => g.Key, g => g.ToArray());
      return new ElementaryValenceSeries(Names, modesDict);
    }

    /// <summary>
    /// Returns the list of compatible valences with all their permutations assigning free names to ground names in the valence list. If the valencevar's name is ground to begin with, that name is returned as the single alternative. Does not modify the names. 
    /// </summary>
    public void GroundingAlternatives(ValenceVar vv, NameVarBindings vvbind, out List<(string[] ins, string[] outs)> groundingAlternatives)
    {
      groundingAlternatives = new();
      if (ModesByModeNumber.TryGetValue(vv.ModeNumber, out ModeIndices[] valenceAlternatives))
      {
        string[] searchedInVars = vvbind.GetNamesForVars(vv.Ins);
        string[] searchedOutVars = vvbind.GetNamesForVars(vv.Outs);
        //OPTIMIZE: There may still be duplicates? If so, it might make sense to distinct the final list of grounding alternatives.
        foreach (ModeIndices valenceAlt in valenceAlternatives)
        {
          if (GroundValence.MatchingAlternatesForNameVars(searchedInVars, Names, valenceAlt.Ins, out var inAlternatives))
          {
            if (GroundValence.MatchingAlternatesForNameVars(searchedOutVars, Names, valenceAlt.Outs, out var outAlternatives))
            {
              if (inAlternatives.Any())
              {
                if (outAlternatives.Any())
                {
                  foreach (var insAlt in inAlternatives)
                    foreach (var outsAlt in outAlternatives)
                      groundingAlternatives.Add((insAlt, outsAlt));
                }
                else
                {
                  groundingAlternatives.AddRange(inAlternatives.Select(a => (a, Array.Empty<string>())));
                }
              }
              else
              {
                groundingAlternatives.AddRange(outAlternatives.Select(o => (Array.Empty<string>(), o)));
              }
            }
            else continue; // no out-unifications
          }
          else continue; // no in-unifications
        }
      }
    }
  }

}