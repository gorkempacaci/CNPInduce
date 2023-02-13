using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using CNP.Helper;
using CNP.Helper.EagerLinq;
using System.Collections;

namespace CNP.Language
{
  /// <summary>
  /// Provides types and factory methods for 'ground' valences, the kind of valences associated with language predicates/operators. This is in contrast to ValenceVariable, that's associated with a search node.
  /// </summary>
  public static class GroundValence
  {

    /// <summary>
    /// In the context of a string[] 'Names', gives which are In and which are Out.
    /// An array of modes gives a valence (with addition of names in CNP).
    /// If a predicate-expression has its In-modes ground, it promises to terminate and its Out-modes to be ground after execution.
    /// Note: it doesn't mean if its In-s aren't ground it won't execute correctly, just that it does not promise to terminate. Therefore every expression built with these valence rules in mind (called well-modedness constraints) then all those expressions have termination guarantee.
    /// </summary>
    /// <param name="ModesArrayIndices"></param>
    public readonly record struct ModeIndices(short[] Ins, short[] Outs)
    {
      public override int GetHashCode() => CalculateValenceModeNumber(Ins.Length, Outs.Length);

    }

    /// <summary>
    /// Pairs fold's mode array to modes of recursive and base components. For fold to guarantee the
    /// mode array (for termination), the components have to guarantee the given mode arrays.
    /// </summary>
    /// <param name="FoldModes"></param>
    /// <param name="RecursiveCaseModes"></param>
    /// <param name="BaseCaseModes"></param>
    public readonly record struct FoldModeIndices(ModeIndices FoldModes, ModeIndices RecursiveCaseModes, ModeIndices BaseCaseModes)
    {
      public override int GetHashCode() => CalculateValenceModeNumber(FoldModes.Ins.Length, FoldModes.Outs.Length);
    }

    /// <summary>
    /// A series of modes all having the same names. Mode[] in the dictionary has the same order as the names in the 'Names' so their length is the same.
    /// </summary>
    public readonly record struct SimpleValenceSeries
    {
      public readonly string[] Names;
      public readonly IReadOnlyDictionary<int, ModeIndices[]> ModesByModeNumber;

      public SimpleValenceSeries(string[] names, IReadOnlyDictionary<int, ModeIndices[]> modesByNumber)
      {
        this.Names = names;
        this.ModesByModeNumber = modesByNumber;
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
            if (matchingAlternatesForNameVars(searchedInVars, Names, valenceAlt.Ins, out var inAlternatives))
            {
              if (matchingAlternatesForNameVars(searchedOutVars, Names, valenceAlt.Outs, out var outAlternatives))
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
      public readonly string[] BaseCaseNames;
      public readonly IReadOnlyDictionary<int, FoldModeIndices[]> FoldModesByModeNumber;

      public FoldValenceSeries(string[] names, string[] recNames, string[] baseNames, IReadOnlyDictionary<int, FoldModeIndices[]> foldModesByNumber)
      {
        this.Names = names;
        this.RecursiveCaseNames = recNames;
        this.BaseCaseNames = baseNames;
        this.FoldModesByModeNumber = foldModesByNumber;
      }

      /// <summary>
      /// Returns the list of compatible valences with all their permutations assigning free names to ground names in the valence list. If the valencevar's name is ground to begin with, that name is returned as the single alternative. Does not modify the names. In the groundingAlternatives, for each alternative, modeIndices for the baseCase and recCase are also returned. The names for these are static so there is no grounding for these.
      /// </summary>
      public void GroundingAlternatives(ValenceVar vv, NameVarBindings vvbind, out List<(string[] ins, string[] outs, ModeIndices rec, ModeIndices bas)> groundingAlternatives)
      {
        groundingAlternatives = new();
        if (FoldModesByModeNumber.TryGetValue(vv.ModeNumber, out FoldModeIndices[] foldValenceAlternatives))
        {
          string[] searchedInVars = vvbind.GetNamesForVars(vv.Ins);
          string[] searchedOutVars = vvbind.GetNamesForVars(vv.Outs);
          //OPTIMIZE: There may still be duplicates? If so, it might make sense to distinct the final list of grounding alternatives.
          foreach (FoldModeIndices valenceAltCombi in foldValenceAlternatives)
          {
            if (matchingAlternatesForNameVars(searchedInVars, Names, valenceAltCombi.FoldModes.Ins, out var inAlternatives))
            {
              if (matchingAlternatesForNameVars(searchedOutVars, Names, valenceAltCombi.FoldModes.Outs, out var outAlternatives))
              {
                foreach (var insAlt in inAlternatives)
                  foreach (var outsAlt in outAlternatives)
                    groundingAlternatives.Add((insAlt, outsAlt, valenceAltCombi.RecursiveCaseModes, valenceAltCombi.BaseCaseModes));
              }
              else continue; // no out-unifications
            }
            else continue; // no in-unifications
          }
        }
      }
    }



    /// <summary>
    /// Used as a consistent indexer for ground valences used for static types, and variable valences used for search.
    /// </summary>
    public static int CalculateValenceModeNumber(int numberOfIns, int numberOfOuts)
    {
      return numberOfIns * 23 + numberOfOuts * 17;
    }

    public static ModeIndices IndicesFromArray(Mode[] modes)
    {
      List<short> inIndices = new();
      List<short> outIndices = new();
      for (short i = 0; i < modes.Length; i++)
      {
        if (modes[i] == Mode.In)
          inIndices.Add(i);
        else outIndices.Add(i);
      }
      return new ModeIndices(inIndices.ToArray(), outIndices.ToArray());
    }

    public static SimpleValenceSeries SeriesFromArrays(string[] Names, Mode[][] arrayOfModeArrays)
    {
      var modesDict = arrayOfModeArrays.Select(ms => IndicesFromArray(ms))
                                       .GroupBy(msi => msi.GetHashCode())
                                       .ToDictionary(g => g.Key, g => g.ToArray());
      return new SimpleValenceSeries(Names, modesDict);
    }

    public static FoldValenceSeries FoldSerieFromArrays(string[] Names, string[] RecursiveCaseNames, string[] BaseCaseNames, (Mode[] FoldModes, Mode[] RecModes, Mode[] BasModes)[] FoldModeTriplesArray)
    {
      var foldModeTriplesIndices = FoldModeTriplesArray.Select(e => new
        FoldModeIndices(IndicesFromArray(e.FoldModes),
                               IndicesFromArray(e.RecModes),
                               IndicesFromArray(e.BasModes)))
                                                       .GroupBy(e => e.GetHashCode())
                                                       .ToDictionary(g => g.Key, g => g.ToArray());
      return new FoldValenceSeries(Names, RecursiveCaseNames, BaseCaseNames, foldModeTriplesIndices);
    }

    


    /// <summary>
    /// Finds ways to match given vars to given candidate names. Produces all combinations of frees to candidates along the way.
    /// </summary>
    /// <param name="vars">string if ground, null if free</param>
    /// <param name="candidateAllNames">All of the names in the candidate type.</param>
    /// <param name="candidateNameIndices">Indices of names available to match.</param>
    /// <param name="bindingAlternatives">[[name1, null, name3], [name1_b, null, name3_b]]. Null appears if that name is not to be bound.</param>
    /// <returns>true with empty alternatives if vars is empty.</returns>
    private static bool matchingAlternatesForNameVars(string[] vars, string[] candidateAllNames, short[] candidateIndices, out string[][] bindingAlternatives)
    {
      if (vars.Length == 0)
      {
        bindingAlternatives = Array.Empty<string[]>();
        return true;
      }
      List<string[]> allAlternatives = new();
      string[] baseAlternative = new string[vars.Length]; // ground part of the alternatives. might look like [null, 'parent', null] where nulls later are replaced with alternations for the free names.
      List<int> freeVarPositions = new List<int>(vars.Length);
      bool[] blockedIndices = new bool[candidateIndices.Length];
      for (int vi = 0; vi < vars.Length; vi++) // for each var to match
      {
        if (vars[vi] == null) // searched name is free, so we'll look for alternatives later with all unblocked candidates
        {
          freeVarPositions.Add(vi);
        }
        else // searched var is ground
        {
          for (int cii = 0; cii < candidateIndices.Length; cii++) // look for a match among candidates
          {
            if (blockedIndices[cii] == false && candidateAllNames[candidateIndices[cii]] == vars[vi])
            { // found a match for the ground name
              baseAlternative[vi] = vars[vi]; // add itself as the ground option
              blockedIndices[cii] = true; // no other vars can be mapped to this index
              goto continueNextVar;
            }
          }
          // ground but no match found.
          bindingAlternatives = null;
          return false;
        }
      continueNextVar:;
      }
      List<short> availableCandidates = new List<short>(candidateIndices.Length);
      for (int cii = 0; cii < candidateIndices.Length; cii++)
      {
        if (blockedIndices[cii] == false)
          availableCandidates.Add(candidateIndices[cii]);
      }
      if (freeVarPositions.Count() == availableCandidates.Count())
      {
        if (freeVarPositions.Count() == 0)
        { // if there are no frees, the ground base become the single alternative. this means the searched names are an exact match to the candidates.
          allAlternatives.Add(baseAlternative);
        }
        else
        {
          var availableCandArr = availableCandidates.ToArray();
          var availableCandPerms = Mathes.Permutations(availableCandArr);
          foreach (short[] aCandidatePerm in availableCandPerms)
          {
            string[] anAlternative = new string[baseAlternative.Length];
            Array.Copy(baseAlternative, anAlternative, baseAlternative.Length);
            for (int fvi = 0; fvi < aCandidatePerm.Length; fvi++)
            {
              anAlternative[fvi] = candidateAllNames[aCandidatePerm[fvi]];
            }
            allAlternatives.Add(anAlternative);
          }
        }
        bindingAlternatives = allAlternatives.ToArray();
        return true;
      }
      else
      { // if searched vars and remaining candidate names can't match then fail.
        bindingAlternatives = null;
        return false;
      }
    }
  }

}