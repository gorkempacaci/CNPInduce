using System;
using System.Collections.Generic;
using CNP.Helper.EagerLinq;
using CNP.Helper;

namespace CNP.Language
{

  /// <summary>
  /// Identity program. Immutable object.
  /// </summary>
  public struct Id : IProgram
  {
    private static GroundValence.SimpleValenceSeries IdValences =
      GroundValence.SeriesFromArrays(new[] { "a", "b" },
                                    new[]
                                    {
                                      new[]{  Mode.In,  Mode.In},
                                      new[]{  Mode.In,  Mode.Out},
                                      new[]{  Mode.Out, Mode.In}
                                    });

    public bool IsClosed => true;

    public override int GetHashCode() => 19;

    public override bool Equals(object obj) => obj is Id;

    public void ReplaceFree(Free _, ITerm __) { }

    public string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    public IProgram Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public ObservedProgram FindLeftmostHole() => null;

    public (ObservedProgram, int) FindRootmostHole(int calleesDistanceToRoot = 0) => (null, int.MaxValue);

    public int GetHeight() => 0;

    public string GetTreeQualifier() => "p";

    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment oldEnv)
    {
      ObservedProgram oldObs = oldEnv.Root.FindHole();
      if (oldObs.Observables.TuplesCount == 0)
        throw new ArgumentException("Id: Observation is empty.");
      List<ProgramEnvironment> programs = new();
      IdValences.GroundingAlternatives(oldObs.Valence, oldEnv.NameBindings, out var alts);
      // at this point we know all rows unified like 'id' should.
      foreach (var alt in alts)
      {
        var currEnv = oldEnv.Clone();
        var currObs = currEnv.Root.FindHole();
        // first try unifying the terms since names don't matter for id
        for (int ri = 0; ri < currObs.Observables.TuplesCount; ri++)
        {
          var tuple = currObs.Observables.Tuples[ri];
          var unifier = new ITerm[2] { tuple[1], tuple[0] };
          if (!currEnv.UnifyInPlace(tuple, unifier))
            return Array.Empty<ProgramEnvironment>();
        }
        if (currEnv.NameBindings.TryBindingAllNamesToGround(currObs.Valence, alt))
        {
          ProgramEnvironment newEnv = currEnv.Clone((currObs, new Id()));
          programs.Add(newEnv);
          if (newEnv.Root is And and && and.LHOperand is Const c)
          {
            ;
          }
        }
      }
      return programs;
    }
  }
}
