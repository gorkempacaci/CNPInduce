using System;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
    public class FoldR : Fold
    {
        public FoldR(Program recursiveCase, Program baseCase) : base(recursiveCase, baseCase) { }
        public override string ToString()
        {
            return "foldr(" + Recursive.ToString() + "," + base.ToString() + ")";
        }
        internal override ObservedProgram FindFirstHole()
        {
            return Base.FindFirstHole() ?? Recursive.FindFirstHole();
        }
        public override Program CloneAndReplace(ObservedProgram oldComponent, Program newComponent, FreeDictionary plannedParenthood)
        {
            return new FoldR(Base.CloneAndReplace(oldComponent, newComponent, plannedParenthood),
                                Recursive.CloneAndReplace(oldComponent, newComponent, plannedParenthood));
        }
        public static IEnumerable<FoldR> FromObservation(ObservedProgram obs)
        {
            if (!obs.ArgumentNames.SetEquals(foldArgumentNames) ||
                !Valences.Fold.TryGetValue(obs.Signature, out IEnumerable<OperatorCombinedSignature> pqSignatures))
            {
                return Iterators.Empty<FoldR>();
            }
            List<AlphaTuple> pObs = new List<AlphaTuple>(), qObs = new List<AlphaTuple>();
            foreach (AlphaTuple at in obs.Observables)
                foldRtoPQ(at["a0"], at["as"], pObs, qObs, at["b"]);
            if (!pObs.Any() || !qObs.Any())
                return Iterators.Empty<FoldR>();
            var newFolds = pqSignatures.Select(op =>
            {
                FreeDictionary fd = new FreeDictionary();
                return new FoldR(new ObservedProgram(pObs.Clone(fd), op.LeftOperandSignature),
                    new ObservedProgram(qObs.Clone(fd), op.RightOperandSignature));
            });
            return newFolds;
        }
        // foldr(P,Q)(A0,[],B) :- Q(A0,B).
        // foldr(P,Q)(A0,[A|At],B) :- foldr(P,Q)(A0,At,Acc), P(A,Acc,B).
        //
        // foldr p q ([], [1,2,3], B) :- q([], B1) , p(3, B1, B2) , p(2, B2, B3), p(1, B3, B).
        static void foldRtoPQ(Term a0, Term @as, List<AlphaTuple> atusP, List<AlphaTuple> atusQ, Term acc)
        {
            if (@as is TermList li)
            {
                Free newAcc = new Free();
                atusP.Add(new AlphaTuple(("a", li.Head), ("b", newAcc), ("ab", acc)));
                foldRtoPQ(a0: a0, @as: li.Tail, atusP: atusP, atusQ: atusQ, acc: newAcc);
            }
            else
            {
                atusQ.Add(new AlphaTuple(("a", a0), ("b", acc)));
            }
        }
    }
}
