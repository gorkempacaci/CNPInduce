using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;

namespace CNP.Language
{

  /// <summary>
  /// A Generic math function that takes a, b, and returns an ab. All integers.
  /// </summary>
  public class GenericMathFunction2 : LibraryProgram
  {
    private static ElementaryValenceSeries PlusValences =
      ElementaryValenceSeries.SeriesFromArrays(new[] { "a", "b", "ab" },
                                    new[]
                                    {
                                          new[]{  Mode.In,  Mode.In, Mode.Out}
                                    });

    Func<int, int, int> function2;

    public GenericMathFunction2(string name, Func<int, int, int> func) : base(name)
    {
      function2 = func;
    }

    protected bool RunElementary(BaseEnvironment env, ITerm[][] tuples, (int a, int b, int ab) indices)
    {
      foreach (var tuple in tuples)
      {
        if (tuple[indices.a] is ConstantTerm ct && ct.Value is int a_int &&
            tuple[indices.b] is ConstantTerm ct2 && ct2.Value is int b_int)
        {
          var unifier = new ITerm[tuple.Length];
          unifier[indices.ab] = new ConstantTerm(function2(a_int, b_int));
          if (!env.UnifyInPlaceIncludingGoal(tuple, unifier, tuples))
            return false;
        }
        else return false;
      }
      return true;
    }

    protected override bool RunElementary(BaseEnvironment env, AlphaRelation rel)
    {
      var indices = rel.GetNameIndices(env.NameBindings, "a", "b", "ab");
      return RunElementary(env, rel.Tuples, indices);
    }

    protected override bool RunElementary(BaseEnvironment env, GroundRelation args)
    {
      var indices = args.GetNameIndices(env.NameBindings, "a", "b", "ab");
      return RunElementary(env, args.Tuples, indices);
    }

    protected override ElementaryValenceSeries Valences => PlusValences;

    protected override LibraryProgram CreateNew()
    {
      return new GenericMathFunction2(Name, function2);
    }


  }
}

