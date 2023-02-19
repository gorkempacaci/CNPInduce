using System;
using System.Collections.Generic;

namespace CNP.Language
{

  public class Plus : LibraryProgram
  {
    private static GroundValence.SimpleValenceSeries PlusValences =
      GroundValence.SeriesFromArrays(new[] { "a", "b", "ab" },
                                    new[]
                                    {
                                          new[]{  Mode.In,  Mode.In, Mode.Out}
                                    });


    public override IProgram Clone(CloningContext cc) => new Plus();
    public override string Pretty(PrettyStringer ps) => "+";

    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment env)
    {
      var empty = Array.Empty<ProgramEnvironment>();
      var obs = env.Root.FindHole();
      PlusValences.GroundingAlternatives(obs.Valence, env.NameBindings, out var alternatives);
      var programs = new List<ProgramEnvironment>(alternatives.Count);
      foreach(var alt in alternatives)
      {
        var newEnv = env.Clone();
        var newObs = newEnv.Root.FindHole();
        if (newEnv.NameBindings.TryBindingAllNamesToGround(newObs.Valence, alt))
        {
          (int a, int b, int ab) = newEnv.NameBindings.GetNameIndices(newObs, "a", "b", "ab");
          foreach (var tuple in newObs.Observables.Tuples)
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
      return programs;
    }
  }
}

