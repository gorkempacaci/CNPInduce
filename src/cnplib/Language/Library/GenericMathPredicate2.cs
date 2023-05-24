using System;
using System.Collections.Generic;

namespace CNP.Language
{

  /// <summary>
  /// A generic math predicate that takes 2 input parameters (named a, b) and succeeds or fails
  /// </summary>
  public class GenericMathPredicate2 : LibraryProgram
  {
    private static ElementaryValenceSeries ValencesMathPredicate2 =
      ElementaryValenceSeries.SeriesFromArrays(new[] { "a", "b" },
                                    new[]
                                    {
                                          new[]{  Mode.In,  Mode.In,}
                                    });

    string name;
    Func<int, int, bool> predicate2;

    public GenericMathPredicate2(string name, Func<int, int, bool> pred)
    {
      this.name = name;
      this.predicate2 = pred;
    }

    public override IProgram Clone(CloningContext cc) => new GenericMathPredicate2(name, predicate2);

    public override string Accept(ICNPVisitor ps) => name;

    public override IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment env)
    {
      var empty = Array.Empty<ProgramEnvironment>();
      var obs = env.Root.FindHole();
      var programs = new List<ProgramEnvironment>();
      for (int oi=0; oi<obs.Observations.Length; oi++)
      {
        ValencesMathPredicate2.GroundingAlternatives(obs.Observations[oi].Valence, env.NameBindings, out var alternatives);

        foreach (var alt in alternatives)
        {
          var newEnv = env.Clone();
          var newObs = newEnv.Root.FindHole();
          if (newEnv.NameBindings.TryBindingAllNamesToGround(newObs.Observations[oi].Valence, alt))
          {
            (int a, int b) = newEnv.NameBindings.GetNameIndices(newObs.Observations[oi], "a", "b");
            foreach (var tuple in newObs.Observations[oi].Examples.Tuples)
            {
              if (tuple[a] is ConstantTerm c_a && tuple[b] is ConstantTerm c_b &&
                  c_a.Value is int valA && c_b.Value is int valB)
              {
                if (!predicate2(valA, valB))
                  return empty;
              }
              else return empty;
            }
            var outEnv = newEnv.Clone((newObs, new GenericMathPredicate2(name, predicate2)));
            programs.Add(outEnv);
          }
        }
      }

      return programs;
    }
  }
}

