using System;
using System.Collections.Generic;
using System.Net.Http;
using CNP.Helper.EagerLinq;
using CNP.Helper;
using System.Diagnostics;

namespace CNP.Language
{

  public class Cons : ElementaryProgram
  {
    private static readonly TypeStore<Valence> valences = TypeHelper.ParseListOfCompactedProgramTypes(new[] {
            "{a:in, b:in, ab:*}",
            "{a:*, b:*, ab:in}"});

    public Cons() { }

 
    protected override Program CloneNode(TermReferenceDictionary plannedParenthood)
    {
      var p = new Cons();
      return p;
    }

    public override bool Equals(object obj)
    {
      return obj is Cons;
    }

    public override int GetHashCode()
    {
      return 31;
    }

    public override string ToString()
    {
      return "cons";
    }


    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<Program> CreateAtFirstHole(Program rootProgram)
    {
      ObservedProgram origObs = rootProgram.FindFirstHole();
      var consTypes = valences.FindCompatibleTypes(origObs.Valence);
      if (!consTypes.Any())
        return Iterators.Empty<Cons>();
      List<Program> programs = new List<Program>();
      foreach (Valence consTypeGround in consTypes)
      {
        var combs = origObs.Valence.PossibleGroundings(consTypeGround);
        foreach (var uni in combs)
        {
          var cloneRoot = rootProgram.CloneAtRoot(uni);
          var cloneObs = cloneRoot.FindFirstHole();
          if (cloneObs.Observables.All(at => Term.UnifyInPlace(at["ab"], new TermList(at["a"], at["b"]))))
          {
            var p = new Cons();
            p.SetFoundingState(rootProgram.CloneAtRoot());
            var newRoot = cloneRoot.CloneAndReplaceObservation(cloneObs, p);
            programs.Add(newRoot);
          }
        }
      }
      return programs;
    }

  }

}
