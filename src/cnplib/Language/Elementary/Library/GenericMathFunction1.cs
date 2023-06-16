using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;

namespace CNP.Language
{

  /// <summary>
  /// A Generic math function that takes a, b, and returns an ab. All integers.
  /// </summary>
  public class GenericMathFunction1 : LibraryProgram
  {
    private ElementaryValenceSeries valence;

    Func<int, int> function1;

    public GenericMathFunction1(string name, Func<int, int> func) : this(name, func, ("a", "b"))
    {

    }

    public GenericMathFunction1(string name, Func<int, int> func, (string, string) argNames) : base(name)
    {
      function1 = func;
      valence = ElementaryValenceSeries.SeriesFromArrays(new[] { argNames.Item1, argNames.Item2 },
                                    new[]
                                    {
                                          new[]{  Mode.In, Mode.Out}
                                    });
    }

    protected bool RunElementary(BaseEnvironment env, ITerm[][] tuples, (int n, int s) indices)
    {
      foreach (var tuple in tuples)
      {
        if (tuple[indices.n] is ConstantTerm ct && ct.Value is int n_int)
        {
          var unifier = new ITerm[tuple.Length];
          unifier[indices.s] = new ConstantTerm(function1(n_int));
          if (!env.UnifyInPlaceIncludingGoal(tuple, unifier, tuples))
            return false;
        }
        else return false;
      }
      return true;
    }

    protected override bool RunElementary(BaseEnvironment env, AlphaRelation rel)
    {
      var indices = rel.GetNameIndices(env.NameBindings, valence.Names[0], valence.Names[1]);
      return RunElementary(env, rel.Tuples, indices);
    }

    protected override bool RunElementary(BaseEnvironment env, GroundRelation args)
    {
      var indices = args.GetNameIndices(env.NameBindings, valence.Names[0], valence.Names[1]);
      return RunElementary(env, args.Tuples, indices);
    }

    protected override ElementaryValenceSeries Valences => valence;

    protected override LibraryProgram CreateNew()
    {
      return new GenericMathFunction1(Name, function1);
    }


  }
}

