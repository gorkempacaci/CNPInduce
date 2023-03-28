using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net.Http;
using System.Threading.Tasks;
using CNP.Helper;
using System.Linq;

namespace CNP.Language
{
  public class FoldR : IFold
  {
    private const Mode In = Mode.In;
    private const Mode Out = Mode.Out;

    public readonly static FoldValenceSeries FoldRValences = FoldValenceSeries.FoldSerieFromArrays(
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

    public FoldR(IProgram recursiveCase)
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
      return "foldr(" + Recursive.GetTreeQualifier() + ")";
    }

    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment env)
    {
      return IFold.CreateAtFirstHole(env, FoldRValences, factoryFoldR, UnFoldR);
    }

    static IFold.CreateFold factoryFoldR = (rec) => new FoldR(rec);


    public static bool UnFoldR(ProgramEnvironment env, AlphaRelation foldRel, (short b0, short @as, short b) nameIndices, FreeFactory freeFac, out ITerm[][] pTuples)
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
            pTuplesList.Add(new ITerm[] { head, seed, result });
          }
          else
          {
            var acc = freeFac.NewFree();
            pTuplesList.Add(new ITerm[] { head, acc, result });
            result = acc;
          }
          list = tail;
        }
        if (list is NilTerm)
        {
          ITerm[] site = foldRel.Tuples[ri];
          ITerm[] unifier = new ITerm[] { null, null, null };
          unifier[nameIndices.b0] = site[nameIndices.b];
        }
        else
        {
          pTuples = null;
          return false;
        }
      }
      if (pTuplesList.Any())
      {
        pTuplesList.Reverse();
        pTuples = pTuplesList.ToArray();
        return true;
      } else
      {
        pTuples = null;
        return false;
      }
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
    //static bool unfoldFoldrToPQ(ITerm b0, ITerm @as, ITerm b, FreeFactory freeFac, List<ITerm[]> atusP, List<ITerm[]> atusQ)
    //{
    //  if (@as is TermList li)
    //  {
    //    Free f = freeFac.NewFree();
    //    atusP.Add(new[] { li.Head, f, b });
    //    return unfoldFoldrToPQ(b0, li.Tail, f, freeFac, atusP, atusQ);
    //  }
    //  else if (@as is NilTerm)
    //  {
    //    atusQ.Add(new[] { b0, b });
    //    return true;
    //  }
    //  else return false;
    //}



  }
}