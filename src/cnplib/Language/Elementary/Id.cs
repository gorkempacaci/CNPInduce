using System;
using System.Collections.Generic;
using CNP.Helper.EagerLinq;
using CNP.Helper;

namespace CNP.Language
{

  /// <summary>
  /// Identity program. Immutable object.
  /// </summary>
  public class Id : ElementaryProgram
  {
    private Id() { }

    public override string ToString()
    {
      return "id";
    }

    internal override Program Clone(TermReferenceDictionary plannedParenthood)
    {
      return this;
    }

    public static readonly Id IdProgram = new();

    private static readonly TypeStore<Valence> valences = TypeHelper.ParseListOfCompactedProgramTypes(new[]
    {
            "{a:in, b:in}",
            "{a:in, b:out}",
            "{a:out, b:in}"
        });

    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<Program> CreateAtFirstHole(Program rootProgramOriginal)
    {

      ObservedProgram obsOriginal = rootProgramOriginal.FindFirstHole();
      var idTypesCompatible = valences.FindCompatibleTypes(obsOriginal.Valence);
      if (!idTypesCompatible.Any())
        return Iterators.Empty<Id>();
      var combs = obsOriginal.Valence.PossibleGroundings(idTypesCompatible.First());
      foreach (var uni in combs)
      {
        Program clonedRoot = rootProgramOriginal.Clone(uni);
        ObservedProgram clonedObs = clonedRoot.FindFirstHole();
        if (clonedObs.Observables.All(at => Term.UnifyInPlace(at["a"], at["b"])))
        {
          return Iterators.Singleton(clonedRoot.CloneAndReplaceObservation(clonedObs, IdProgram));
        }
      }
      return Iterators.Empty<Id>();
    }
  }
}
