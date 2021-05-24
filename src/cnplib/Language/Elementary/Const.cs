using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
  //TODO: if straight semantics are implemented, const(a, 5) should unify with proj(a, id), or any tuple that has {a:5}.
  public class Const : ElementaryProgram
  {
    public NameVar ArgumentName { get; private set; }
    public Term Value { get; private set; }


    public Const(NameVar argName, Term groundTerm)
    {
      if (!groundTerm.IsGround())
      {
        throw new Exception("The term for the Const can only be a ground term.");
      }

      ArgumentName = argName;
      Value = groundTerm;
    }

    internal override Program CloneAsSubTree(TermReferenceDictionary plannedParenthood, (ObservedProgram, Program) replaceObservation)
    {
      var p = new Const(ArgumentName.Clone(plannedParenthood) as NameVar, Value.Clone(plannedParenthood));
      return p;
    }

    public override bool Equals(object obj)
    {
      if (obj is null || !(obj is Const constProgram))
        return false;
      bool sameName = constProgram.ArgumentName.Equals(this.ArgumentName);
      bool sameValue = constProgram.Value.Equals(this.Value);
      return sameName && sameValue;
    }

    public override int GetHashCode()
    {
      return this.Value.GetHashCode();
    }

    public override string ToString()
    {
      return "const(" + ArgumentName + ", " + Value.ToString() + ")";
    }

    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<Program> CreateAtFirstHole(Program rootProgram)
    {
      rootProgram = rootProgram.CloneAtRoot();
      ObservedProgram cloneObs = rootProgram.FindFirstHole();
      if (cloneObs.Valence.Count() != 1)
        return Iterators.Empty<Const>();
      Free candidateConstant = new Free();
      NameVar argName = cloneObs.Valence.Names.First();
      var allTups = Enumerable.ToList(cloneObs.Observables);
      int count = allTups.Count();
      for (int i = 1; i < count; i++)
        if (!Term.UnifyInPlace(allTups[0][argName], allTups[i][argName]))
          return Iterators.Empty<Const>();
      if (!allTups[0][argName].IsGround())
        return Iterators.Empty<Const>(); // has to be ground otherwise can't use CloneAndReplaceWithClosed
      var constProgram = new Const(argName, allTups[0][argName]);
      rootProgram = rootProgram.CloneAtRoot((cloneObs, constProgram));
      return Iterators.Singleton(rootProgram);
    }

  }
}
