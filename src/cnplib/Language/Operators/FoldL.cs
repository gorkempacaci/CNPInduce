using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
    public class FoldL : Fold
    {
        public FoldL(Program recursiveCase, Program baseCase) : base(recursiveCase, baseCase) { }
        public override string ToString()
        {
            return "foldl(" + Recursive.ToString() + "," + Base.ToString() + ")";
        }
        internal override ObservedProgram FindFirstHole()
        {
            return Recursive.FindFirstHole() ?? Base.FindFirstHole();
        }
        public override Program CloneAndReplace(TermReferenceDictionary plannedParenthood, ObservedProgram oldComponent,
            Program newComponent)
        {
            return new FoldL(Base.CloneAndReplace(plannedParenthood, oldComponent, newComponent),
                             Recursive.CloneAndReplace(plannedParenthood, oldComponent, newComponent));
        }

        private static TypeStore<FoldLType> valences =
            TypeHelper.ParseCompactOperatorTypes<FoldLType>(new[]
            {
                "{a:*, b:*, ab:out} -> {a:*, b:out} -> {a0:in, bs:in, a:out}",
                "{a:*, b:*, ab:out} -> {a:out, b:out} -> {a0:out, bs:in, a:out}",
                "{a:out, b:*, ab:out} -> {a:*, b:out} -> {a0:in, bs:out, a:out}",
                "{a:out, b:*, ab:out} -> {a:out, b:out} -> {a0:out, bs:out, a:out}"
            });
        
        public static IEnumerable<FoldL> FromObservation(ObservedProgram obs)
        {
            return Iterators.Empty<FoldL>();
            if (!valences.TryGetValue(obs.Domains, out IEnumerable<FoldLType> pqTypes))
            {
                return Iterators.Empty<FoldL>();
            }
            List<AlphaTuple> pObs = new List<AlphaTuple>(), qObs = new List<AlphaTuple>();
            foreach (AlphaTuple at in obs.Observables)
                foldLtoPQ(at["a0"], at["bs"], at["a"], pObs, qObs);
            if (!pObs.Any() || !qObs.Any())
                return Iterators.Empty<FoldL>();
            var newFolds = pqTypes.Select(op =>
            {
                TermReferenceDictionary fd = new TermReferenceDictionary();
                return new FoldL(recursiveCase:new ObservedProgram(pObs.Clone(fd), op.RecursiveComponentDomains),
                    baseCase:new ObservedProgram(qObs.Clone(fd), op.BaseComponentDomains));
            });
            return newFolds;
        }
        // foldl(P,Q)(A0,nil,B) :- Q(A0,B).
        // foldl(P,Q)(A0,[A|At],B) :- P(A,A0,Acc), foldl(P,Q)(Acc,At,B).
        // reverse3([], [1,2,3], [3,2,1]) :- id([], []), cons(1, [], [1]), cons(2, [1], [2,1]). cons(3, [2,1], [3,2,1]).
        //
        // foldl p q ([], [1,2,3], B) :- q([], B1), p(1, B1, B2), p(2, B2, B3), p(3, B3, B)
        static void foldLtoPQ(Term a0, Term bs, Term a, List<AlphaTuple> atusP, List<AlphaTuple> atusQ, Term acc = null)
        {
            if (acc == null)
            {
                Free f = new Free();
                atusQ.Add(new AlphaTuple(("a", a0), ("b", f)));
                foldLtoPQ(a0, bs, a, atusP, atusQ, f);
            }
            else if (bs is TermList li)
            {
                if (li.Tail is TermList)
                {
                    Free f = new Free();
                    atusP.Add(new AlphaTuple(("a", li.Head), ("b", acc), ("ab", f)));
                    foldLtoPQ(a0, li.Tail, a, atusP, atusQ, f);
                }
                else
                {
                    atusP.Add(new AlphaTuple(("a", li.Head), ("b", acc), ("ab", a)));
                }

            }
        }
    }
}
/*
foldl(a0, [], b) :- q(a0, b).
foldl(a0, [a|as], b) :- foldl(a0, as, b1), p(a, b1, b).
foldl([], [1,2,3], b) :- q([], []), p(3, [], [3]), p(2, [3], [2,3]), p(1, [2,3], [1,2,3])

foldl(Y,[],Z) :- q(Y,Z).
foldl(Y,[X|T],W) :- p(X,Y,Z), foldl(Z,T,W).
foldl([], [1,2,3], [3,2,1]) :- p(1, [], [1]), p(2, [1], [2,1]), p(3, [2,1], [3,2,1]), q([3,2,1], [3,2,1]).

foldr :: (a -> b -> b) -> b -> [a] -> b

foldl :: (a -> b -> a) -> a -> [b] -> a

foldl(p,q)(a0, [], a) :- q(a0, a).
foldl(p,q)(a0, [b|bs], a) :- p(a0, b, b2), foldl(b2, bs, a). 

 * 
 */
