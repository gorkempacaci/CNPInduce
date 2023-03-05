using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using CNP.Helper;
using System.Linq;
using System.Collections;

namespace CNP.Language
{


  /// <summary>
  /// Provides types and factory methods for 'ground' valences, the kind of valences associated with language predicates/operators. This is in contrast to ValenceVariable, that's associated with a search node.
  /// </summary>
  public static class GroundValence
  {
    /// <summary>
    /// Used as a consistent indexer for ground valences used for static types, and variable valences used for search.
    /// </summary>
    public static int CalculateValenceModeNumber(int numberOfIns, int numberOfOuts)
    {
      return numberOfIns * 23 + numberOfOuts * 17;
    }


    /// <summary>
    /// Finds ways to match given vars to given candidate names. Produces all combinations of frees to candidates along the way.
    /// </summary>
    /// <param name="vars">string if ground, null if free</param>
    /// <param name="candidateAllNames">All of the names in the candidate type.</param>
    /// <param name="candidateNameIndices">Indices of names available to match.</param>
    /// <param name="bindingAlternatives">[[name1, null, name3], [name1_b, null, name3_b]]. Null appears if that name is not to be bound.</param>
    /// <returns>true with empty alternatives if vars is empty.</returns>
    internal static bool MatchingAlternatesForNameVars(string[] vars, string[] candidateAllNames, short[] candidateIndices, out string[][] bindingAlternatives)
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
              anAlternative[freeVarPositions[fvi]] = candidateAllNames[aCandidatePerm[fvi]];
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