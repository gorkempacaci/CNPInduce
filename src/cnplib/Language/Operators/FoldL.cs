using System;
using System.Collections.Generic;
using CNP.Language;
using CNP.Helper;
using System.Linq;

namespace CNP.Language
{
  public class FoldL : IFold
  {
    private const Mode In = Mode.In;
    private const Mode Out = Mode.Out;

    //BUG: FOLDL valences are same as foldr
    public readonly static GroundValence.FoldValenceSeries FoldLValences = GroundValence.FoldSerieFromArrays(
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

    public FoldL(IProgram recursiveCase, IProgram baseCase)
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
      return "foldl(" + Recursive.GetTreeQualifier() + "," + Base.GetTreeQualifier() + ")";
    }

    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment env)
    {
      return IFold.CreateAtFirstHole(env, FoldLValences, factoryFoldL, UnFoldL);
    }

    static IFold.CreateFold factoryFoldL = (rec, bas) => new FoldL(rec, bas);


    public static bool UnFoldL(AlphaRelation foldRel, (short b0, short @as, short b) nameIndices, FreeFactory freeFac, out ITerm[][] pTuples, out ITerm[][] qTuples)
    {
      List<ITerm[]> pTuplesList = new();
      List<ITerm[]> qTuplesList = new();
      for(int ri=0; ri<foldRel.TuplesCount; ri++)
      {
        ITerm b0 = foldRel.Tuples[ri][nameIndices.b0];
        ITerm @as = foldRel.Tuples[ri][nameIndices.@as];
        ITerm b = foldRel.Tuples[ri][nameIndices.b];
        if (!unfoldFoldlToPQ(b0, @as, b, freeFac, pTuplesList, qTuplesList))
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
with names indicating types:
foldl(Bn, [], B) :- Q(Bn, B).
foldl(B0, [A|As], B) :- P(A, B0, B1), foldl(B1, As, B).
if B is int, A is char, P is (char > int > int), P(a,0,1), P(b,0,2), P(c,0,3), Q=id.
foldl(0, [a, b, c], B) :- P(a, 0, B1), foldl(B1, [b, c], B)
 :- P(a, 0, B1), P(b, B1, B2), foldl(B2, [c], B)
 :- P(a, 0, B1), P(b, B1, B2), P(B2, c, B3), foldl(B3, [], B)
 :- P(a, 0, B1), P(b, B1, B2), P(B2, c, B3), Q(B3, B).
                     :- P(a, 0, 1),  P(b, 1, 3),   P(3, c, 6),   Q(6, 6).
                     :- foldl(0, [a, b, c], 6).
reverse3([], [1,2,3], Bz)
== foldl(cons, id)([], [1,2,3], Bz)
== cons(1, [], B1), foldl(cons, id)(B1, [2,3], Bz)
== cons(1, [], B1), cons(2, B1, B2), foldl(cons, id)(B2, [3], Bz)
== cons(1, [], B1), cons(2, B1, B2), cons(3, B2, B3), foldl(cons, id)(B3, [], Bz)
== cons(1, [], B1), cons(2, B1, B2), cons(3, B2, B3), id(B3, Bz)
== cons(1, [], [1]), cons(2, [1], [2,1]), cons(3, [2,1], [3,2,1]), id([3,2,1], [3,2,1]).
== foldl(cons, id)([], [1,2,3], [3,2,1])
reverse3([], [1,2,3], [3,2,1])
*/
    /// <summary>
    /// lists atusP and atusQ should be initialized before call, since they're populated by this function.
    /// </summary>
    /// <returns></returns>
    static bool unfoldFoldlToPQ(ITerm b0, ITerm @as, ITerm b, FreeFactory freeFac, List<ITerm[]> atusP, List<ITerm[]> atusQ)
    {
      if (@as is TermList li)
      {
        Free f = freeFac.NewFree();
        atusP.Add(new[] { li.Head, b0, f }); //a, b, ab
        return unfoldFoldlToPQ(f, li.Tail, b, freeFac, atusP, atusQ);
      }
      else if (@as is NilTerm)
      {
        atusQ.Add(new[] { b0, b }); //a, b
        return true;
      }
      else return false;
    }
  }
}
// /*
// foldl(a0, [], b) :- q(a0, b).
// foldl(a0, [a|as], b) :- foldl(a0, as, b1), p(a, b1, b).
// foldl([], [1,2,3], b) :- q([], []), p(3, [], [3]), p(2, [3], [2,3]), p(1, [2,3], [1,2,3])
//
// foldl(Y,[],Z) :- q(Y,Z).
// foldl(Y,[X|T],W) :- p(X,Y,Z), foldl(Z,T,W).
// foldl([], [1,2,3], [3,2,1]) :- p(1, [], [1]), p(2, [1], [2,1]), p(3, [2,1], [3,2,1]), q([3,2,1], [3,2,1]).
//
// foldr :: (a -> b -> b) -> b -> [a] -> b
//
// foldl :: (a -> b -> a) -> a -> [b] -> a
//
// foldl(p,q)(a0, [], a) :- q(a0, a).
// foldl(p,q)(a0, [b|bs], a) :- p(a0, b, b2), foldl(b2, bs, a). 
//
//  * 
//  */
