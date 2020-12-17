using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net.Http;
using System.Threading.Tasks;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
  public class FoldR : Fold
  {
    private static TypeStore<FoldType> valences =
        TypeHelper.ParseCompactOperatorTypes<FoldType>(
            new[]
            {       // valence for P -> valence for Q -> valence for foldr(P,Q)
                    "{a:*, b:*, ab:out} -> {a:*, b:out} -> {b0:in, as:in, b:out}",
                    "{a:*, b:*, ab:out} -> {a:out, b:out} -> {b0:out, as:in, b:out}",
                    "{a:out, b:*, ab:out} -> {a:*, b:out} -> {b0:in, as:out, b:out}",
                    "{a:out, b:*, ab:out} -> {a:out, b:out} -> {b0:out, as:out, b:out}"
            });

    public FoldR(Program recursiveCase, Program baseCase) : base(recursiveCase, baseCase) { }
    public override string ToString()
    {
      return "foldr(" + Recursive.ToString() + "," + Base.ToString() + ")";
    }

    internal override FoldR Clone(TermReferenceDictionary plannedParenthood)
    {
      return new FoldR(Recursive.Clone(plannedParenthood),
          Base.Clone(plannedParenthood));
    }

    internal override FoldR CloneAndReplaceObservation(ObservedProgram oldComponent, Program newComponent, TermReferenceDictionary plannedParenthood)
    {
      return new FoldR(Recursive.CloneAndReplaceObservation(oldComponent, newComponent, plannedParenthood),
        Base.CloneAndReplaceObservation(oldComponent, newComponent, plannedParenthood));
    }

    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<Program> CreateAtFirstHole(Program rootProgram)
    {
      return Fold.CreateAtFirstHole(rootProgram, valences as TypeStore<FoldType>, (rec, bas) => new FoldR(rec, bas), unfoldFoldrToPQ);
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
    static bool unfoldFoldrToPQ(Term b0, Term @as, Term b, List<AlphaTuple> atusP, List<AlphaTuple> atusQ)
    {
      if (@as is TermList li)
      {
        Free f = new();
        atusP.Add(new AlphaTuple(("a", li.Head), ("b",f), ("ab",b)));
        return unfoldFoldrToPQ(b0, li.Tail, f, atusP, atusQ);
      }
      else if (@as is NilTerm)
      {
        atusQ.Add(new AlphaTuple(("a", b0), ("b",b)));
        return true;
      }
      else return false;
    }
    //static bool unfoldFoldrToPQ(Term b0, Term @as, Term b, List<AlphaTuple> atusP, List<AlphaTuple> atusQ, Term acc = null)
    //{
    //  if (acc == null)
    //  {
    //    acc = b;
    //  }
    //  if (@as is TermList li)
    //  {
    //    Free f = new Free();
    //    atusP.Add(new AlphaTuple(("a", li.Head), ("b", f), ("ab", acc)));
    //    return unfoldFoldrToPQ(b0, li.Tail, b, atusP, atusQ, f);
    //  }
    //  else if (@as is NilTerm)
    //  {
    //    atusQ.Add(new AlphaTuple(("a", b0), ("b", acc)));
    //    return true;
    //  }
    //  else return false;
    //}
  }
}
