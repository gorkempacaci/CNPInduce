using System;
using System.Collections.Generic;
using CNP.Helper.EagerLinq;
using CNP.Helper;
using CNP.Display;

namespace CNP.Language
{

  /// <summary>
  /// Identity program. Immutable object.
  /// </summary>
  public class Id : ElementaryProgram
  {
    private static readonly TypeStore<Valence> valences = TypeHelper.ParseListOfCompactedProgramTypes(new[]
    {
            "{a:in, b:in}",
            "{a:in, b:out}",
            "{a:out, b:in}"
        });

    public Id() { }

    internal override Program CloneAsSubTree(TermReferenceDictionary plannedParenthood, (ObservedProgram, Program) replaceObservation)
    {
      return new Id();
    }

    public override int GetHashCode()
    {
      return 19;
    }

    public override bool Equals(object obj)
    {
      return obj is Id;
    }

    public override string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<Program> CreateAtFirstHole(Program rootProgramOriginal)
    {
      ObservedProgram obsOriginal = rootProgramOriginal.FindHole();
      var idTypesCompatible = valences.FindCompatibleTypes(obsOriginal.Valence);
      if (!idTypesCompatible.Any())
        return Iterators.Empty<Id>();

      var combs = obsOriginal.Valence.PossibleGroundings(idTypesCompatible.First());
      foreach (TermReferenceDictionary uni in combs)
      {
        Program clonedRoot = rootProgramOriginal.CloneAtRoot(uni);
        if (!clonedRoot.NameConstraintsHold())
          continue; //rollback
        ObservedProgram clonedObs = clonedRoot.FindHole();
        if (clonedObs.Observables.All(at => Term.UnifyInPlace(at["a"], at["b"])))
        {
          var p = new Id();
          //if (rootProgramOriginal.ToString().StartsWith("proj(and("))
          //{
          //  And ann = (rootProgramOriginal as Proj).Source as And;
          //  if ((ann.LHOperand as ObservedProgram).Valence.Count()==2 &&
          //      (ann.RHOperand as ObservedProgram).Valence.Count()==2)
          //  {

          //  }
          //}
          return Iterators.Singleton(clonedRoot.CloneAtRoot((clonedObs, p)));
        }
      }

      return Iterators.Empty<Id>();
    }
  }
}
