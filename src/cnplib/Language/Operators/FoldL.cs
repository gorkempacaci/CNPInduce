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
        public override Program CloneAndReplace(ObservedProgram oldComponent, Program newComponent, FreeDictionary plannedParenthood)
        {
            return new FoldL(Base.CloneAndReplace(oldComponent, newComponent, plannedParenthood),
                             Recursive.CloneAndReplace(oldComponent, newComponent, plannedParenthood));
        }
        public static IEnumerable<FoldL> FromObservation(ObservedProgram obs)
        {
            if (!obs.ArgumentNames.SetEquals(foldArgumentNames) ||
                !Valences.Fold.TryGetValue(obs.Signature, out IEnumerable<OperatorCombinedSignature> pqSignatures))
            {
                return Iterators.Empty<FoldL>();
            }
            List<AlphaTuple> pObs = new List<AlphaTuple>(), qObs = new List<AlphaTuple>();
            foreach (AlphaTuple at in obs.Observables)
                foldLtoPQ(at["a0"], at["as"], at["b"], pObs, qObs);
            if (!pObs.Any() || !qObs.Any())
                return Iterators.Empty<FoldL>();
            var newFolds = pqSignatures.Select(op =>
            {
                FreeDictionary fd = new FreeDictionary();
                return new FoldL(new ObservedProgram(pObs.Clone(fd), op.LeftOperandSignature),
                    new ObservedProgram(qObs.Clone(fd), op.RightOperandSignature));
            });
            return newFolds;
        }
        // foldl(P,Q)(A0,nil,B) :- Q(A0,B).
        // foldl(P,Q)(A0,[A|At],B) :- P(A,A0,Acc), foldl(P,Q)(Acc,At,B).
        // reverse3([], [1,2,3], [3,2,1]) :- id([], []), cons(1, [], [1]), cons(2, [1], [2,1]). cons(3, [2,1], [3,2,1]).
        //
        // foldl p q ([], [1,2,3], B) :- q([], B1), p(1, B1, B2), p(2, B2, B3), p(3, B3, B)
        static void foldLtoPQ(Term a0, Term @as, Term b, List<AlphaTuple> atusP, List<AlphaTuple> atusQ, Term acc = null)
        {
            if (acc == null)
            {
                Free f = new Free();
                atusQ.Add(new AlphaTuple(("a", a0), ("b", f)));
                foldLtoPQ(a0, @as, b, atusP, atusQ, f);
            }
            else if (@as is TermList li)
            {
                if (li.Tail is TermList)
                {
                    Free f = new Free();
                    atusP.Add(new AlphaTuple(("a", li.Head), ("b", acc), ("ab", f)));
                    foldLtoPQ(a0, li.Tail, b, atusP, atusQ, f);
                }
                else
                {
                    atusP.Add(new AlphaTuple(("a", li.Head), ("b", acc), ("ab", b)));
                }

            }
        }
    }
}
