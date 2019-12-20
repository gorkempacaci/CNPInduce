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
            return "foldl(" + Recursive.ToString() + "," + base.ToString() + ")";
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
                foldLtoPQ(at["a0"], at["as"], pObs, qObs, at["b"]);
            if (pObs.Any() || qObs.Any())
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
        //
        // foldl p q ([], [1,2,3], B) :- p(1, [], B1), p(2, B1, B2), p(3, B2, B3), q(B3, B).
        static void foldLtoPQ(Term @as, Term b, List<AlphaTuple> atusP, List<AlphaTuple> atusQ, Term acc)
        {
            if (@as is TermList li)
            {
                Free newAcc = new Free();
                atusP.Add(new AlphaTuple(("a", li.Head), ("b", acc), ("ab", newAcc)));
                foldLtoPQ(@as: li.Tail, @b: b, @atusP: atusP, @atusQ: atusQ, acc: newAcc);
            }
            else
            {
                atusQ.Add(new AlphaTuple(("a", acc), ("b", b)));
            }
        }
    }
}
