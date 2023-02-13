using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net.Http;
using System.Threading.Tasks;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
  public class FoldR : IFold
  {
    private const Mode In = Mode.In;
    private const Mode Out = Mode.Out;

    public readonly static GroundValence.FoldValenceSeries FoldRValences = GroundValence.FoldSerieFromArrays(
      new[] { "b0", "as", "b" }, new[] { "a", "b", "ab" }, new[] { "a", "b" },
      new[] {
        (new[]{In, In, Out}, new[]{In, In, Out }, new[]{In, Out }),
        (new[]{In, In, Out}, new[]{In, Out, Out }, new[]{In, Out }),
        (new[]{In, In, Out}, new[]{Out, In, Out }, new[]{In, Out }),
        (new[]{In, In, Out}, new[]{Out, Out, Out }, new[]{In, Out }),
        (new[]{In, In, Out}, new[]{In, In, Out }, new[]{Out, Out }),
        (new[]{In, In, Out}, new[]{In, Out, Out }, new[]{Out, Out }),
        (new[]{In, In, Out}, new[]{Out, In, Out }, new[]{Out, Out }),
        (new[]{In, In, Out}, new[]{Out, Out, Out }, new[]{Out, Out }),

        (new[]{Out,In,Out}, new[]{In,In,Out}, new[]{Out, Out }),
        (new[]{Out,In,Out}, new[]{In,Out,Out}, new[]{Out, Out }),
        (new[]{Out,In,Out}, new[]{Out,In,Out}, new[]{Out, Out }),
        (new[]{Out,In,Out}, new[]{Out,Out,Out}, new[]{Out, Out }),

        (new[]{In, Out, Out}, new[]{Out, In, Out}, new[]{In, Out}),
        (new[]{In, Out, Out}, new[]{Out, Out, Out}, new[]{In, Out}),
        (new[]{In, Out, Out}, new[]{Out, In, Out}, new[]{Out, Out}),
        (new[]{In, Out, Out}, new[]{Out, Out, Out}, new[]{Out, Out}),

        (new[]{Out, Out, Out}, new[]{Out, In, Out}, new[]{Out, Out}),
        (new[]{Out, Out, Out}, new[]{Out, Out, Out}, new[]{Out, Out})
      });

    public IProgram Recursive { get; }
    public IProgram Base { get; }

    public FoldR(IProgram recursiveCase, IProgram baseCase)
    {
      Recursive = recursiveCase;
      Base = baseCase;
    }

    public void ReplaceFree(Free free, ITerm term)
    {
      Recursive.ReplaceFree(free, term);
      Base.ReplaceFree(free, term);
    }

    public string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    public IProgram Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public string GetTreeQualifier()
    {
      return "foldr(" + Recursive.GetTreeQualifier() + "," + Base.GetTreeQualifier() + ")";
    }

    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment env)
    {
      return IFold.CreateAtFirstHole(env, FoldRValences, factoryFoldR, UnFoldR);
    }

    static IFold.CreateFold factoryFoldR = (rec, bas) => new FoldR(rec, bas);


    public static bool UnFoldR(AlphaRelation foldRel, (short b0, short @as, short b) nameIndices, FreeFactory freeFac, out ITerm[][] pTuples, out ITerm[][] qTuples)
    {
      List<ITerm[]> pTuplesList = new();
      List<ITerm[]> qTuplesList = new();
      for (int ri = 0; ri < foldRel.TuplesCount; ri++)
      {
        ITerm b0 = foldRel.Tuples[ri][nameIndices.b0];
        ITerm @as = foldRel.Tuples[ri][nameIndices.@as];
        ITerm b = foldRel.Tuples[ri][nameIndices.b];
        if (!unfoldFoldrToPQ(b0, @as, b, freeFac, pTuplesList, qTuplesList))
        {
          pTuples = null;
          qTuples = null;
          return false;
        }
      }
      pTuples = pTuplesList.ToArray();
      qTuples = qTuplesList.ToArray();
      if (pTuples.Any() && qTuples.Any())
        return true;
      else return false;
    }

    /*
    foldr(B0, [], B1) :- Q(B0, B1).
    foldr(B0, [A|As], B) :- foldr(B0, As, Bi), P(A, Bi, B).

    append([4,5,6], [1,2,3], L)
    == foldr(cons, id)([4,5,6], [1,2,3], L)
    == foldr(cons, id)([4,5,6], [2,3], Li), cons(1, Li, L).
    == foldr(cons, id)([4,5,6], [3], Lj),   cons(2, Lj, Li), cons(1, Li, L).
    == foldr(cons, id)([4,5,6], [], Lk),    cons(3, Lk, Lj), cons(2, Lj, Li), cons(1, Li, L).
    == id([4,5,6], Lk),                     cons(3, Lk, Lj), cons(2, Lj, Li), cons(1, Li, L).
    == id([4,5,6], [4,5,6]),                cons(3, [4,5,6], [3,4,5,6]), cons(2, [3,4,5,6], [2,3,4,5,6]), cons(1, [2,3,4,5,6], [1,2,3,4,5,6]).
    == foldr(cons, id)([4,5,6], [1,2,3], [1,2,3,4,5,6]).
    == append([4,5,6], [1,2,3], [1,2,3,4,5,6]).

           from oldager paper:
           foldl(Y, [], Z) :- Q(Y, Z).
           foldl(Y, [X|T], W) :- P(X, Y, Z), foldl(Z, T, W).
     
           foldr(Y, [], Z) :- Q(Y, Z).
           foldr(Y, [X|T], W) :- foldr(Y, T, Z), P(X, Z, W).
     */
    static bool unfoldFoldrToPQ(ITerm b0, ITerm @as, ITerm b, FreeFactory freeFac, List<ITerm[]> atusP, List<ITerm[]> atusQ)
    {
      if (@as is TermList li)
      {
        Free f = freeFac.NewFree();
        atusP.Add(new[] { li.Head, f, b });
        return unfoldFoldrToPQ(b0, li.Tail, f, freeFac, atusP, atusQ);
      }
      else if (@as is NilTerm)
      {
        atusQ.Add(new[] { b0, b });
        return true;
      }
      else return false;
    }



  }
}