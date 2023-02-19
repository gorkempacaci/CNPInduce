using System;
using CNP.Language;

namespace CNP.Language
{
  /// <summary>
  /// Couples a root program to a context (names for name variables, and counting for frees)
  /// </summary>
  public class ProgramEnvironment
  {
    public readonly NameVarBindings NameBindings;
    public readonly FreeFactory Frees;
    public IProgram Root { get; private set; }

    /// <summary>
    /// Set to false if an in-place unification fails for this environment.
    /// </summary>
    public bool Dirty { get => _dirty; set { _dirty |= value; } }
    private bool _dirty = false;
    
    public ProgramEnvironment(IProgram prog, NameVarBindings nameVars, FreeFactory frf)
    {
      this.Root = prog;
      this.NameBindings = nameVars;
      this.Frees = frf;
    }

    public void ReplaceFree(Free free, ITerm term)
    {
      Root.ReplaceFree(free, term);
    }


    /// <summary>
    /// Overwrites the terms in the 'site' if necessary to unify them with terms in the unifier, in their given order. Performs substitutions through the given env, so they apply to the whole root context. Mutates the given siteRow and its environment so it needs to be disposed if the unification fails. Skips positions i where unifierRow[i] is null.
    /// </summary>
    public bool UnifyInPlace(ITerm[] siteRow, ITerm[] unifierRow)
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
            ReplaceFree(suggSubstitution.Value.Item1, suggSubstitution.Value.Item2);
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

    public ProgramEnvironment Clone()
    {
      if (Dirty)
        throw new InvalidOperationException("ProgramEnvironment is dirty.");
      return Clone(null, null);
    }

    public ProgramEnvironment Clone((ObservedProgram, IProgram)? observationReplacement)
    {
      return Clone(observationReplacement, null);
    }

    public ProgramEnvironment Clone((ObservedProgram,IProgram)? observationReplacement, (NameVar[], NameVar[])? AssertedDifferences)
    {
      if (Dirty)
        throw new InvalidOperationException("ProgramEnvironment is dirty.");
      CloningContext cc = new CloningContext(this.NameBindings, this.Frees);
      cc.ObservationReplacement = observationReplacement;
      var p = this.Root.Clone(cc);
      if (AssertedDifferences.HasValue)
      {
        cc.AssertDifferencesUsingOldIndices(AssertedDifferences.Value.Item1, AssertedDifferences.Value.Item2);
      }
      return new ProgramEnvironment(p, cc.NewNameBindings, cc.NewFreeFactory);
    }

  }
}

