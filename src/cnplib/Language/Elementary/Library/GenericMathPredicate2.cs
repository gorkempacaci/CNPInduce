using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;

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

    Func<int, int, bool> predicate2;

    public GenericMathPredicate2(string name, Func<int, int, bool> pred) : base(name)
    {
      this.predicate2 = pred;
    }

    protected bool RunElementary(ITerm[][] tuples, (int a, int b) indices)
    {
      foreach (var tuple in tuples)
      {
        if (tuple[indices.a] is ConstantTerm ct && ct.Value is int a_int &&
            tuple[indices.b] is ConstantTerm ct2 && ct2.Value is int b_int)
        {
          var unifier = new ITerm[tuple.Length];
          if (!predicate2(a_int, b_int))
            return false;
        }
        else return false;
      }
      return true;
    }

    protected override bool RunElementary(BaseEnvironment env, GroundRelation args)
    {
      var indices = args.GetNameIndices(env.NameBindings, "a", "b");
      return RunElementary(args.Tuples, indices);
    }

    protected override bool RunElementary(BaseEnvironment env, AlphaRelation args)
    {
      var indices = args.GetNameIndices(env.NameBindings, "a", "b");
      return RunElementary(args.Tuples, indices);
    }

    protected override ElementaryValenceSeries Valences => ValencesMathPredicate2;

    protected override LibraryProgram CreateNew()
    {
      return new GenericMathPredicate2(Name, predicate2);
    }
  }
}

