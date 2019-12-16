using System;
using System.Collections.Generic;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{

    public class Cons : ElementaryProgram
    {
        public static readonly Cons ConsProgram = new Cons();
        private Cons() { }
        private static readonly ISet<string> consArgNames = new HashSet<string>() { "a", "b", "ab" };
        private static readonly IEnumerable<Signature> consModes = new List<Signature>
        {
            new Signature( ("a", ArgumentMode.In), ("b", ArgumentMode.In), ("ab", ArgumentMode.Out) ),
            new Signature( ("a", ArgumentMode.Out), ("b", ArgumentMode.Out), ("ab", ArgumentMode.In) )
        };
        public override ISet<string> ArgumentNames => consArgNames;
        public override string ToString()
        {
            return "cons";
        }
        public static Cons FromObservation(ObservedProgram op)
        {

            if (!op.ArgumentNames.SetEquals(consArgNames) || !consModes.Contains(op.Signature))
            {
                return null;
            }
            if (op.Observables.All(at => Term.UnifyInPlace(at["ab"], new TermList(at["a"], at["b"]))))
                return ConsProgram;
            else return null;
        }

    }

}
