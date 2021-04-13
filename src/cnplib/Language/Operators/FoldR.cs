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
    private static TypeStore<FoldValence> valences =
        TypeHelper.ParseListOfCompactedComposedTypes<FoldValence>(
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

    protected override Program CloneNode(TermReferenceDictionary plannedParenthood)
    {
      var p = new FoldR(Recursive.CloneAsSubTree(plannedParenthood), Base.CloneAsSubTree(plannedParenthood));
      return p;
    }

    protected override FoldR CloneAndReplaceObservationAtNode(ObservedProgram oldComponent, Program newComponent, TermReferenceDictionary plannedParenthood)
    {
      var p = new FoldR(Recursive.CloneAndReplaceObservationAsSubTree(oldComponent, newComponent, plannedParenthood),
        Base.CloneAndReplaceObservationAsSubTree(oldComponent, newComponent, plannedParenthood));
      return p;
    }

    /// <summary>
    /// Does not modify the given program, returns alternative cloned programs if they exist.
    /// </summary>
    public static IEnumerable<Program> CreateAtFirstHole(Program rootProgram)
    {
      Func<Program, Program, Fold> factoryFoldR = (rec, bas) => new FoldR(rec, bas);
      return Fold.CreateAtFirstHole(rootProgram, valences as TypeStore<FoldValence>, factoryFoldR, unfoldFoldrToPQ);
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
    public static bool unfoldFoldrToPQ(Term b0, Term @as, Term b, List<AlphaTuple> atusP, NameVarDictionary pNameDict, List<AlphaTuple> atusQ, NameVarDictionary qNameDict)
    {
      if (@as is TermList li)
      {
        Free f = new();
        atusP.Add(new AlphaTuple((pNameDict.GetOrAdd("a"), li.Head),
                                 (pNameDict.GetOrAdd("b"),f),
                                 (pNameDict.GetOrAdd("ab"),b)));
        return unfoldFoldrToPQ(b0, li.Tail, f, atusP, pNameDict, atusQ, qNameDict);
      }
      else if (@as is NilTerm)
      {
        atusQ.Add(new AlphaTuple((qNameDict.GetOrAdd("a"), b0),
                                 (qNameDict.GetOrAdd("b"),b)));
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
