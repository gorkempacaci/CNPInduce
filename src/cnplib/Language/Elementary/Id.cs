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
    private static ElementaryValenceSeries IdValences =
      ElementaryValenceSeries.SeriesFromArrays(new[] { "a", "b" },
                                    new[]
                                    {
                                      new[]{  Mode.In,  Mode.In},
                                      new[]{  Mode.In,  Mode.Out},
                                      new[]{  Mode.Out, Mode.In}
                                    });

    /// <summary>
    /// The valence that lead to this program.
    /// </summary>
    public string DebugValenceString { get; set; }
    /// <summary>
    /// The observations that lead to this program.
    /// </summary>
    public string DebugObservationString { get; set; }

    public bool IsClosed => true;

    public override int GetHashCode() => 19;

    public override bool Equals(object obj) => obj is Id;

    public void ReplaceFree(Free _, ITerm __) { }

    public string Accept(ICNPVisitor ps)
    {
      return ps.Visit(this);
    }

    public IProgram Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public ObservedProgram FindLeftmostHole() => null;

    public int GetHeight() => 0;

    public string GetTreeQualifier() => "p";

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

        IdValences.GroundingAlternatives(oldObs.Observations[oi].Valence, oldEnv.NameBindings, out var alts);
        // at this point we know all rows unified like 'id' should.
        foreach (var alt in alts)
        {
          var currEnv = oldEnv.Clone();
          var currObs = currEnv.Root.FindHole();
          // first try unifying the terms since names don't matter for id
          for (int ri = 0; ri < currObs.Observations[oi].Examples.TuplesCount; ri++)
          {
            var tuple = currObs.Observations[oi].Examples.Tuples[ri];
            var unifier = new ITerm[2] { tuple[1], tuple[0] };
            if (!currEnv.UnifyInPlace(tuple, unifier))
              return Array.Empty<ProgramEnvironment>();
          }
          if (currEnv.NameBindings.TryBindingAllNamesToGround(currObs.Observations[oi].Valence, alt))
          {
            ProgramEnvironment newEnv = currEnv.Clone((currObs, new Id()));
            programs.Add(newEnv);
            if (newEnv.Root is And and && and.LHOperand is Const c)
            {
              ;
            }
          }
        }
      }
      return programs;
    }
  }
}
