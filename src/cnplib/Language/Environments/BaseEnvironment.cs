using System;
using System.Collections.Generic;
using CNP.Language;

namespace CNP.Language
{
  public abstract class BaseEnvironment
  {
    public IProgram Root { get; private set; }
    public NameVarBindings NameBindings { get; init; }
    public readonly FreeFactory Frees;

    public BaseEnvironment(IProgram root, NameVarBindings nvb, FreeFactory ff)
    {
      this.Root = root;
      this.NameBindings = nvb;
      this.Frees = ff;
    }

    public abstract void ReplaceFree(Free free, ITerm term);

    /// <summary>
    /// Set to false if an in-place unification fails for this environment.
    /// </summary>
    public bool Dirty { get => _dirty; set { _dirty |= value; } }
    private bool _dirty = false;


    /// <summary>
    /// Takes a set of tuples and unifies each using the unifier delegate.
    /// Also, applies the substitutions to the given goalTuples (since they're not a part of the ExecutionEnvironment)
    /// </summary>
    public bool UnifyInPlaceAllTuples(ITerm[][] tuples, Func<ITerm[], ITerm[]> f_unifier, ITerm[][] goalTuples)
    {
      foreach (var tuple in tuples)
      {
        if (!(UnifyInPlaceIncludingGoal(tuple, f_unifier(tuple), goalTuples)))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Overwrites the terms in the 'site' if necessary to unify them with terms in the unifier, in their given order. Performs substitutions through the given env, so they apply to the whole root context. Mutates the given siteRow and its environment so it needs to be disposed if the unification fails. Skips positions i where unifierRow[i] is null.
    /// </summary>
    public bool UnifyInPlace(ITerm[] siteRow, ITerm[] unifierRow)
    {
      return UnifyInPlaceIncludingGoal(siteRow, unifierRow, null);
    }

    /// <summary>
    /// Takes a tuple and applies the unifier tuple.
    /// Also, applies the substitutions to the given goalTuples (since they're not a part of the ExecutionEnvironment)
    /// </summary>
    public bool UnifyInPlaceIncludingGoal(ITerm[] siteRow, ITerm[] unifierRow, IEnumerable<ITerm[]> goalTuples)
    {
      for (int ei = 0; ei < siteRow.Length; ei++)
      {
        if (unifierRow[ei] is null)
          continue;
        while (false == ITerm.UnifyOrSuggest(siteRow[ei], unifierRow[ei], out var suggSubstitution))
        {
          if (suggSubstitution.HasValue)
          { // needs a substitution
            // for the siterow's env
            ReplaceFree(suggSubstitution.Value.Item1, suggSubstitution.Value.Item2);
            // for the siterow itself (during execution the siterow might not be in env.
            ReplaceFreesInPlaceInTuple(siteRow, suggSubstitution.Value);
            // for unifierRow
            ReplaceFreesInPlaceInTuple(unifierRow, suggSubstitution.Value);
            // for the goal relation
            if (goalTuples != null)
              foreach(var goalTuple in goalTuples)
                ReplaceFreesInPlaceInTuple(goalTuple, suggSubstitution.Value);
          }
          else
          { // no way for unification for this tuple, therefore no way for the whole relation.
            Dirty = true;
            return false;
          }
        }
      }
      return true;
    }

    static void ReplaceFreesInPlaceInTuple(ITerm[] tuple, (Free, ITerm) replacement)
    {
      for (int i = 0; i < tuple.Length; i++)
        if (tuple[i] != null)
          tuple[i] = tuple[i].GetFreeReplaced(replacement.Item1, replacement.Item2);
    }
  }
}

