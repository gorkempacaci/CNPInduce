using System;
using System.Collections.Generic;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{

    /// <summary>
    /// Identity program. Immutable object.
    /// </summary>
    public class Id : ElementaryProgram
    {
        public static readonly Id IdProgram = new Id();
        private Id() { }
        private static readonly ISet<string> IdArgNames = new HashSet<string>() {"a", "b"};
        public override ISet<string> ArgumentNames => IdArgNames;

        public override string ToString()
        {
            return "id";
        }

        public static Id FromObservation(ObservedProgram obs)
        {
            if (!IdArgNames.SetEquals(obs.ArgumentNames) || !Valences.Id.Contains(obs.Signature))
                return null;

            if (obs.Observables.All(at => Term.UnifyInPlace(at.Terms["a"], at.Terms["b"])))
            {
                return IdProgram;
            }
            else
            {
                return null;
            }
            
        }

    }
}
