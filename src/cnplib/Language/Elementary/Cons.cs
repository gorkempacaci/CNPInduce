using System;
using System.Collections.Generic;
using CNP.Helper.EagerLinq;
using  CNP.Helper;

namespace CNP.Language
{

    public class Cons : ElementaryProgram
    {
        public static readonly Cons ConsProgram = new Cons();
        private Cons() { }
        private static readonly ISet<string> consArgNames = new HashSet<string>() { "a", "b", "ab" };

        public override ISet<string> ArgumentNames => consArgNames;
        public override string ToString()
        {
            return "cons";
        }
        public static IEnumerable<Cons> FromObservation(ObservedProgram op)
        {

            if (!op.ArgumentNames.SetEquals(consArgNames) || !Valences.Cons.Contains(op.Signature))
                return Iterators.Empty<Cons>();

            if (op.Observables.All(at => Term.UnifyInPlace(at["ab"], new TermList(at["a"], at["b"]))))
                return Iterators.Singleton(ConsProgram);
            else return Iterators.Empty<Cons>();
        }

    }

}
