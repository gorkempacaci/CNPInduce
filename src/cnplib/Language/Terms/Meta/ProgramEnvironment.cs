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

