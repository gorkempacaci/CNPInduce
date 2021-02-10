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
      ObservedProgram origObs = rootProgram.FindFirstHole();
      var consTypes = valences.FindCompatibleTypes(origObs.Valence);
      if (!consTypes.Any())
        return Iterators.Empty<Cons>();
      var combs = origObs.Valence.PossibleGroundings(consTypes.First());
      foreach(var uni in combs)
      {
        var cloneRoot = rootProgram.Clone(uni);
        var cloneObs = cloneRoot.FindFirstHole();
        if (cloneObs.Observables.All(at => Term.UnifyInPlace(at["ab"], new TermList(at["a"], at["b"]))))
          return Iterators.Singleton(cloneRoot.CloneAndReplaceObservation(cloneObs, ConsProgram));
      }
      return Iterators.Empty<Program>();
    }

  }

}
