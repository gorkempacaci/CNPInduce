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
        public FoldR(Program recursiveCase, Program baseCase) : base(recursiveCase, baseCase) { }
        public override string ToString()
        {
            return "foldr(" + Recursive.ToString() + "," + Base.ToString() + ")";
        }
        internal override ObservedProgram FindFirstHole()
        {
            return Base.FindFirstHole() ?? Recursive.FindFirstHole();
        }
        public override Program CloneAndReplace(TermReferenceDictionary plannedParenthood, ObservedProgram oldComponent,
            Program newComponent)
        {
            return new FoldR(Base.CloneAndReplace(plannedParenthood, oldComponent, newComponent),
                                Recursive.CloneAndReplace(plannedParenthood, oldComponent, newComponent));
        }

        private static TypeStore<FoldRType> valences =
            TypeHelper.ParseCompactOperatorTypes<FoldRType>(
                new[]
                {
                    "{a:*, b:*, ab:out} -> {a:*, b:out} -> {b0:in, as:in, b:out}",
                    "{a:*, b:*, ab:out} -> {a:out, b:out} -> {b0:out, as:in, b:out}",
                    "{a:out, b:*, ab:out} -> {a:*, b:out} -> {b0:in, as:out, b:out}",
                    "{a:out, b:*, ab:out} -> {a:out, b:out} -> {b0:out, as:out, b:out}"
                });
        
        public static IEnumerable<FoldR> FromObservation(ObservedProgram obs)
        {
            if (!valences.TryGetValue(obs.Domains, out IEnumerable<FoldRType> foldTypes))
            {
                return Iterators.Empty<FoldR>();
            }
            IEnumerable<ObservedProgram> newObservedPrograms = obs.CloneToGroundDomains(foldTypes.First().Domains);
            List<FoldR> newFoldrs = new List<FoldR>();
            foreach (ObservedProgram groundObs in newObservedPrograms)
            {
                List<AlphaTuple> pObs = new List<AlphaTuple>(), qObs = new List<AlphaTuple>();
                foreach (AlphaTuple at in groundObs.Observables)
                    foldRtoPQ(at["b0"], at["as"], at["b"], pObs, qObs);
                if (!pObs.Any() || !qObs.Any())
                    return Iterators.Empty<FoldR>();

                newFoldrs.AddRange(foldTypes.Select(op =>
                {
                    TermReferenceDictionary fd = new TermReferenceDictionary();
                    return new FoldR(new ObservedProgram(pObs.Clone(fd), op.RecursiveComponentDomains),
                        new ObservedProgram(qObs.Clone(fd), op.BaseComponentDomains));
                }));
            }
            return newFoldrs;
        }

        // foldr(P,Q)(A0,[],B) :- Q(A0,B).
        // foldr(P,Q)(A0,[A|At],B) :- foldr(P,Q)(A0,At,Acc), P(A,Acc,B).
        static void foldRtoPQ(Term b0, Term @as, Term b, List<AlphaTuple> atusP, List<AlphaTuple> atusQ, Term acc = null)
        {
            if (acc == null)
            {
                acc = b;
            }
            if (@as is TermList li)
            {
                Free f = new Free();
                atusP.Add(new AlphaTuple(("a", li.Head), ("b", f), ("ab", acc)));
                foldRtoPQ(b0, li.Tail, b, atusP, atusQ, f);
            } else if (@as is NilTerm)
            {
                atusQ.Add(new AlphaTuple(("a", b0), ("b", acc)));
            } else throw new Exception("foldRtoPQ: as is not list or nil.");
        }
    }
}
