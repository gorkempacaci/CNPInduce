using System;
using System.Collections.Generic;
using CNP.Helper.EagerLinq;
using CNP.Helper;
using System.Linq;

namespace CNP.Language
{

  /// <summary>
  /// Identity program. Immutable object.
  /// </summary>
  public class Id : ElementaryProgram
  {
    internal static ElementaryValenceSeries IdValences =
      ElementaryValenceSeries.SeriesFromArrays(new[] { "a", "b" },
                                    new[]
                                    {
                                      new[]{  Mode.In,  Mode.In},
                                      new[]{  Mode.In,  Mode.Out},
                                      new[]{  Mode.Out, Mode.In}
                                    });

    public override bool Equals(object obj) => obj is Id;

    public override int GetHashCode() => 29;

    public override string Accept(ICNPVisitor ps) => ps.Visit(this);

    public override IProgram Clone(CloningContext cc) => cc.Clone(this);

    public override string[] GetGroundNames(NameVarBindings nvb) => IdValences.Names;

    protected override bool RunElementary(BaseEnvironment env, GroundRelation args)
    {
      return RunStatic(env, args);
    }

    private static bool RunStatic(BaseEnvironment env, RelationBase rel)
    {
      //TODO: unifier should be {[1], null} instead of {[1], [0]} for efficiency.
      return env.UnifyInPlaceAllTuples(rel.Tuples, tuple => new[] { tuple[1], tuple[0] }, rel.Tuples);
    }

    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment oldEnv)
    {
      ObservedProgram oldObs = oldEnv.Root.FindHole();
      List<ProgramEnvironment> programs = new();
      for (int oi = 0; oi < oldObs.Observations.Length; oi++)
      {
        if (oldObs.Observations[oi].Examples.TuplesCount == 0)
          throw new ArgumentException("Id: Observation is empty.");
        if (oldObs.Observations[oi].Examples.ColumnsCount != 2)
          return Array.Empty<ProgramEnvironment>();
        if (!RunStatic(oldEnv, oldObs.Observations[oi].Examples))
          return Array.Empty<ProgramEnvironment>();
        // at this point we know all rows unified like 'id' should.
        IdValences.GroundingAlternatives(oldObs.Observations[oi].Valence, oldEnv.NameBindings, out var alts);
        foreach (var alt in alts)
        {
#if DEBUG
          if (alts.Count > 2)
            throw new ArgumentOutOfRangeException("There should only be max 2 alts for id.");
#endif
            var currEnv = oldEnv.Clone();
          var currObs = currEnv.Root.FindHole();
          if (currEnv.NameBindings.TryBindingAllNamesToGround(currObs.Observations[oi].Valence, alt))
          {
            ProgramEnvironment newEnv = currEnv.Clone((currObs, new Id()));
            programs.Add(newEnv);
          }
        }
      }
      return programs;
    }
  }
}
