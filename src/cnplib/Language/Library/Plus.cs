using System;
using System.Collections.Generic;

namespace CNP.Language
{

  public class Plus : LibraryProgram
  {
    /*
     Other library predicates:
    - lt
    - lte
    - smallerOf : {a, b, ab} == ab=a if a<=b, else ab=b. (then min=fold(smallerOf))
    
     */
    private static ElementaryValenceSeries PlusValences =
      ElementaryValenceSeries.SeriesFromArrays(new[] { "a", "b", "ab" },
                                    new[]
                                    {
                                          new[]{  Mode.In,  Mode.In, Mode.Out}
                                    });


    public override IProgram Clone(CloningContext cc) => new Plus();
    public override string Accept(ICNPVisitor ps) => "+";

    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment env)
    {
      var empty = Array.Empty<ProgramEnvironment>();
      var obs = env.Root.FindHole();
      var programs = new List<ProgramEnvironment>();
      for (int oi=0; oi<obs.Observations.Length; oi++)
      {
        PlusValences.GroundingAlternatives(obs.Observations[oi].Valence, env.NameBindings, out var alternatives);

        foreach (var alt in alternatives)
        {
          var newEnv = env.Clone();
          var newObs = newEnv.Root.FindHole();
          if (newEnv.NameBindings.TryBindingAllNamesToGround(newObs.Observations[oi].Valence, alt))
          {
            (int a, int b, int ab) = newEnv.NameBindings.GetNameIndices(newObs.Observations[oi], "a", "b", "ab");
            foreach (var tuple in newObs.Observations[oi].Examples.Tuples)
            {
              if (tuple[a] is ConstantTerm c_a && tuple[b] is ConstantTerm c_b &&
                  c_a.Value is int valA && c_b.Value is int valB)
              {
                int valAB = valA + valB;
                var unifierTuple = new ITerm[] { null, null, new ConstantTerm(valAB) };
                if (!newEnv.UnifyInPlace(tuple, unifierTuple))
                  return empty;
              }
              else return empty;
            }
            var outEnv = newEnv.Clone((newObs, new Plus()));
            programs.Add(outEnv);
          }
        }
      }

      return programs;
    }
  }
}

