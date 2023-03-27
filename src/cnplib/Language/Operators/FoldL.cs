using System;
using System.Collections.Generic;
using CNP.Language;
using CNP.Helper;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CNP.Language
{
  public class FoldL : IFold
  {
    private const Mode In = Mode.In;
    private const Mode Out = Mode.Out;

    //BUG: FOLDL valences are same as foldr
    public readonly static FoldValenceSeries FoldLValences = FoldValenceSeries.FoldSerieFromArrays(
      new[] { "b0", "as", "b" }, new[] { "a", "b", "ab" },
      new[] {
        (new[]{In, In, Out}, new[]{In, In, Out }),
        (new[]{In, In, Out}, new[]{In, Out, Out }),
        (new[]{In, In, Out}, new[]{Out, In, Out }),
        (new[]{In, In, Out}, new[]{Out, Out, Out }),

        (new[]{Out,In,Out}, new[]{In,Out,Out}),
        (new[]{Out,In,Out}, new[]{Out,Out,Out}),

        (new[]{In, Out, Out}, new[]{Out, In, Out}),
        (new[]{In, Out, Out}, new[]{Out, Out, Out}),

        (new[]{Out, Out, Out}, new[]{Out, Out, Out})
      });

    public IProgram Recursive { get; }

    /// <summary>
    /// The valence that lead to this program.
    /// </summary>
    public string DebugValenceString { get; set; }
    /// <summary>
    /// The observations that lead to this program.
    /// </summary>
    public string DebugObservationString { get; set; }

    public FoldL(IProgram recursiveCase)
    {
      Recursive = recursiveCase;
    }


    ObservedProgram IProgram.FindLeftmostHole()
    {
      return Recursive.FindLeftmostHole();
    }

    public void ReplaceFree(Free free, ITerm term)
    {
      Recursive.ReplaceFree(free, term);
    }

    public string Accept(ICNPVisitor ps)
    {
      return ps.Visit(this);
    }

    public IProgram Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public string GetTreeQualifier()
    {
      return "foldl(" + Recursive.GetTreeQualifier() + ")";
    }

    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment env)
    {
      return IFold.CreateAtFirstHole(env, FoldLValences, factoryFoldL, UnFoldL);
    }

    static IFold.CreateFold factoryFoldL = (rec) => new FoldL(rec);


    public static bool UnFoldL(AlphaRelation foldRel, (short b0, short @as, short b) nameIndices, FreeFactory freeFac, out ITerm[][] pTuples)
    {
      List<ITerm[]> pTuplesList = new();
      for (int ri = 0; ri < foldRel.TuplesCount; ri++)
      {
        ITerm seed = foldRel.Tuples[ri][nameIndices.b0];
        ITerm list = foldRel.Tuples[ri][nameIndices.@as];
        ITerm result = foldRel.Tuples[ri][nameIndices.b];
        while (list is TermList termList)
        {
          ITerm head = termList.Head;
          ITerm tail = termList.Tail;
          if (tail is NilTerm)
          {
            pTuplesList.Add(new ITerm[] { head, seed, result }); // a, b, ab
          }
          else
          {
            Free carry = freeFac.NewFree();
            pTuplesList.Add(new ITerm[] { head, seed, carry }); // a, b, ab
            seed = carry;
          }
          list = tail;
        }
      }
      if (pTuplesList.Any())
      {
        pTuples = pTuplesList.ToArray();
        return true;
      }
      else
      {
        pTuples = null;
        return false;
      }
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
