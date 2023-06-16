using System;
using System.Collections.Generic;
using System.Net.Http;
using CNP.Helper.EagerLinq;
using CNP.Helper;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;

namespace CNP.Language
{

  public class Cons : ElementaryProgram
  {
    internal static ElementaryValenceSeries ConsValences =
  ElementaryValenceSeries.SeriesFromArrays(new[] { "a", "b", "ab" },
                                new[]
                                {
                                      new[]{  Mode.In,  Mode.In, Mode.In},
                                      new[]{  Mode.In,  Mode.In, Mode.Out},
                                      new[]{  Mode.In, Mode.Out, Mode.In},
                                      new[]{  Mode.Out, Mode.In, Mode.In},
                                      new[]{  Mode.Out, Mode.Out, Mode.In},
                                });


    public override int GetHashCode() => 31;

    public override bool Equals(object obj) => obj is Cons;

    public override string Accept(ICNPVisitor ps) => ps.Visit(this);

    public override IProgram Clone(CloningContext cc) => cc.Clone(this);

    public override string[] GetGroundNames(NameVarBindings nvb)
    {
      return ConsValences.Names;
    }

    protected override bool RunElementary(BaseEnvironment env, GroundRelation args)
    {
      (int a, int b, int ab) nameIndices = args.GetNameIndices(env.NameBindings, "a", "b", "ab");
      return RunStatic(env, args.Tuples, nameIndices);
    }

    private static bool RunStatic(BaseEnvironment env, ITerm[][] tuples, (int a, int b, int ab) nameIndices)
    {
      foreach (var tuple in tuples)
      {
        var unifier = new ITerm[tuple.Length];
        unifier[nameIndices.ab] = new TermList(tuple[nameIndices.a], tuple[nameIndices.b]);
        if (!env.UnifyInPlaceIncludingGoal(tuple, unifier, tuples))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment env)
    {
      List<ProgramEnvironment> programs = new();
      ObservedProgram oldObs = env.Root.FindHole();
      for(int oi=0; oi<oldObs.Observations.Length; oi++)
      {
        var obs = oldObs.Observations[oi];
        ConsValences.GroundingAlternatives(obs.Valence, env.NameBindings, out var alts);
        foreach (var altNames in alts)
        {
          var currEnv = env.Clone();
          var observ = currEnv.Root.FindHole();
          var obsAlt = observ.Observations[oi];
          if (currEnv.NameBindings.TryBindingAllNamesToGround(obsAlt.Valence, altNames))
          { // then all names for valence are ground
            var rel = obsAlt.Examples;
            (int a, int b, int ab) = rel.GetNameIndices(currEnv.NameBindings, "a", "b", "ab");

            var success = RunStatic(currEnv, obsAlt.Examples.Tuples, (a, b, ab)); 
            if (success && obsAlt.IsAllOutArgumentsGround())
            {
              var debugInfo = obsAlt.GetDebugInformation(currEnv);
              debugInfo.valenceString += $" [{a}|{b}]={ab}";
              Cons cns = new Cons();
              (cns as IProgram).SetDebugInformation(debugInfo);
              programs.Add(currEnv.Clone((observ, cns)));
            }
          } // if not then this alt is skipped
        }
      }
      
      return programs;
    }

  }

}
