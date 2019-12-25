using System;
using System.Collections.Generic;
using CNP.Helper.EagerLinq;
using CNP.Helper;

namespace CNP.Language
{

    /// <summary>
    /// Identity program. Immutable object.
    /// </summary>
    public class Id : ElementaryProgram
    {
        private Id() {}
        
        public override string ToString()
        {
            return "id";
        }
        public static readonly Id IdProgram = new Id();

        private static readonly IReadOnlyCollection<ProgramType> valences = TypeHelper.ParseCompactProgramTypes(new[]
        {
            "{a:in, b:*}",
            "{a:*, b:in}"
        });

        public static IEnumerable<Id> FromObservation(ObservedProgram obs)
        {
            if (!valences.Contains(obs.ProgramType))
                return Iterators.Empty<Id>();

            if (obs.Observables.All(at => Term.UnifyInPlace(at.Terms["a"], at.Terms["b"])))
                return Iterators.Singleton(IdProgram);
            else return Iterators.Empty<Id>();

        }
    }
}
