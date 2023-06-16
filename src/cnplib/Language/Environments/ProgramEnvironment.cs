using System;
using CNP.Language;

namespace CNP.Language
{
  /// <summary>
  /// Couples a root program to a context (names for name variables, and counting for frees)
  /// </summary>
  public class ProgramEnvironment : BaseEnvironment
  {
    
    public ProgramEnvironment(IProgram prog, NameVarBindings nameVars, FreeFactory frf) : base(prog, nameVars, frf)
    {
      
    }

    public override void ReplaceFree(Free free, ITerm term)
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

    public ProgramEnvironment Clone((ObservedProgram,IProgram)? observationReplacement, (NameVar[], NameVar[])[] AssertedDifferences)
    {
      if (Dirty)
        throw new InvalidOperationException("ProgramEnvironment is dirty.");
      CloningContext cc = new CloningContext(this.NameBindings, this.Frees);
      cc.ObservationReplacement = observationReplacement;
      var p = this.Root.Clone(cc);
      if (AssertedDifferences is not null)
      {
        //for (int i=0; i<AssertedDifferences.Length; i++)
        //  cc.AssertDifferencesUsingOldIndices(AssertedDifferences[i].Item1, AssertedDifferences[i].Item2);
      }
      return new ProgramEnvironment(p, cc.NewNameBindings, cc.NewFreeFactory);
    }

    public ExecutionEnvironment ToExecutionEnvironment()
    {
      if (Dirty)
        throw new InvalidOperationException("ProgramEnvironment is dirty.");
      CloningContext cc = new CloningContext(this.NameBindings, this.Frees);
      var newRoot = this.Root.Clone(cc);
      ExecutionEnvironment exenv = new ExecutionEnvironment(newRoot, cc.NewNameBindings, cc.NewFreeFactory);
      return exenv;
    }
  }
}

