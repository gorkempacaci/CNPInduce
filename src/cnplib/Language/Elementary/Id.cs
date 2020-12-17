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

    private static readonly TypeStore<ProgramType> valences = TypeHelper.ParseCompactProgramTypes(new[]
    {
            "{a:in, b:*}",
            "{a:*, b:in}"
        });

    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<Program> CreateAtFirstHole(Program rootProgram)
    {
      rootProgram = rootProgram.Clone();
      ObservedProgram obs = rootProgram.FindFirstHole();
      if (!valences.FindCompatibleTypes(obs.Domains).Any())
        return Iterators.Empty<Id>();

      if (obs.Observables.All(at => Term.UnifyInPlace(at["a"], at["b"])))
      {
        return Iterators.Singleton(rootProgram.CloneAndReplaceObservation(obs, IdProgram));
      } else return Iterators.Empty<Id>();
    }
  }
}
