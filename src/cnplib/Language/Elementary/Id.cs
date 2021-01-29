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
    public static IEnumerable<Program> CreateAtFirstHole(Program rootProgram)
    {
      rootProgram = rootProgram.Clone();
      ObservedProgram obs = rootProgram.FindFirstHole();
      var idTypesCompatible = valences.FindCompatibleTypes(obs.Valence);
      if (!idTypesCompatible.Any())
        return Iterators.Empty<Id>();
        //TODO: this only binds names in a,b order. Is it necessary to also do it the other way around?
      var combs = obs.Valence.PossibleGroundings(idTypesCompatible.First());
      obs.Valence.First().Key.BindName("a");
      obs.Valence.Skip(1).First().Key.BindName("b");
      if (obs.Observables.All(at => Term.UnifyInPlace(at["a"], at["b"])))
      {
        return Iterators.Singleton(rootProgram.CloneAndReplaceObservation(obs, IdProgram));
      } else return Iterators.Empty<Id>();
    }
  }
}
