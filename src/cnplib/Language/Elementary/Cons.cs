using System;
using System.Collections.Generic;
using System.Net.Http;
using CNP.Helper.EagerLinq;
using CNP.Helper;

namespace CNP.Language
{

  public class Cons : ElementaryProgram
  {
    private static readonly TypeStore<Valence> valences = TypeHelper.ParseListOfCompactedProgramTypes(new[] {
            "{a:in, b:in, ab:*}",
            "{a:*, b:*, ab:in}"});

    private Cons() { }

    public override string ToString()
    {
      return "cons";
    }

    internal override Program Clone(TermReferenceDictionary plannedParenthood)
    {
      return this;
    }

    public static readonly Cons ConsProgram = new Cons();
    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<Program> CreateAtFirstHole(Program rootProgram)
    {
      Program p = rootProgram.Clone();
      ObservedProgram obs = p.FindFirstHole();
      if (!valences.FindCompatibleTypes(obs.Valence).Any())
        return Iterators.Empty<Cons>();
      //if (!obs.Valence.Names.Contains(new NameVar("ab")))
      //{
      //  throw new InvalidOperationException();
      //}
      if (obs.Observables.All(at => Term.UnifyInPlace(at["ab"], new TermList(at["a"], at["b"]))))
        return Iterators.Singleton(p.CloneAndReplaceObservation(obs, ConsProgram));
      else return Iterators.Empty<Program>();
    }

  }

}
