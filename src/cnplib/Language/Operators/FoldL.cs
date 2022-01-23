﻿using System;
using System.Collections.Generic;
using CNP.Display;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
  public class FoldL : Fold
  {
    private static TypeStore<FoldValence> valences =
        TypeHelper.ParseListOfCompactedComposedTypes<FoldValence>(new[]
        {
                 "{a:*, b:*, ab:out} -> {a:*, b:out} -> {b0:in, as:in, b:out}",
                 "{a:*, b:*, ab:out} -> {a:out, b:out} -> {b0:out, as:in, b:out}",
                 "{a:out, b:*, ab:out} -> {a:*, b:out} -> {b0:in, as:out, b:out}",
                 "{a:out, b:*, ab:out} -> {a:out, b:out} -> {b0:out, as:out, b:out}"
        });

    public FoldL(Program recursiveCase, Program baseCase) : base(recursiveCase, baseCase) { }


    public override string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    internal override Program CloneAsSubTree(TermReferenceDictionary plannedParenthood, (ObservedProgram, Program) replaceObservation = default)
    {
      var p = new FoldL(Recursive.CloneAsSubTree(plannedParenthood, replaceObservation), Base.CloneAsSubTree(plannedParenthood, replaceObservation));
      return p;
    }


    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<Program> CreateAtFirstHole(Program rootProgram)
    {
      Func<Program, Program, Fold> factoryFoldL = (rec, bas) => new FoldL(rec, bas);
      return Fold.CreateAtFirstHole(rootProgram, valences, factoryFoldL, unfoldFoldlToPQ);
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
    public static bool unfoldFoldlToPQ(Term b0, Term @as, Term b, List<AlphaTuple> atusP, NameVarDictionary pNameDict, List<AlphaTuple> atusQ, NameVarDictionary qNameDict)
    {
      if (@as is TermList li)
      {
        Free f = new Free();
        atusP.Add(new AlphaTuple((pNameDict.GetOrAdd("a"), li.Head),
                                 (pNameDict.GetOrAdd("b"), b0),
                                 (pNameDict.GetOrAdd("ab"), f)));
        return unfoldFoldlToPQ(f, li.Tail, b, atusP, pNameDict, atusQ, qNameDict);
      }
      else if (@as is NilTerm)
      {
        atusQ.Add(new AlphaTuple((qNameDict.GetOrAdd("a"), b0),
                                 (qNameDict.GetOrAdd("b"), b)));
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
