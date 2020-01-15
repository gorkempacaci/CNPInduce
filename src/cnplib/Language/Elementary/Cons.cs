using System;
using System.Collections.Generic;
using System.Net.Http;
using CNP.Helper.EagerLinq;
using  CNP.Helper;

namespace CNP.Language
{

    public class Cons : ElementaryProgram
    {
        private Cons() { }

        public override string ToString()
        {
            return "cons";
        }

        private static readonly TypeStore<ProgramType> valences = TypeHelper.ParseCompactProgramTypes(new[]
        {
            "{a:in, b:in, ab:*}",
            "{a:*, b:*, ab:in}"
        });
        
        public static readonly Cons ConsProgram = new Cons();
        public static IEnumerable<Cons> FromObservation(ObservedProgram op)
        {
            if (!valences.TryGetValue(op.Domains, out _))
                return Iterators.Empty<Cons>();
            if (!op.Domains.Names.Contains(new ArgumentName("ab")))
            {
                throw null;
            }
            if (op.Observables.All(at => Term.UnifyInPlace(at["ab"], new TermList(at["a"], at["b"]))))
                return Iterators.Singleton(ConsProgram);
            else return Iterators.Empty<Cons>();
        }

    }

}
