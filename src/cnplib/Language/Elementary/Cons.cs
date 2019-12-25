using System;
using System.Collections.Generic;
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

        private static readonly IReadOnlyCollection<ProgramType> valences = TypeHelper.ParseCompactProgramTypes(new[]
        {
            "{a:in, b:in, ab:*}",
            "{a:*, b:*, ab:in}"
        });
        
        public static readonly Cons ConsProgram = new Cons();
        public static IEnumerable<Cons> FromObservation(ObservedProgram op)
        {

            if (!valences.Contains(op.ProgramType))
                return Iterators.Empty<Cons>();

            if (op.Observables.All(at => Term.UnifyInPlace(at["ab"], new TermList(at["a"], at["b"]))))
                return Iterators.Singleton(ConsProgram);
            else return Iterators.Empty<Cons>();
        }

    }

}
