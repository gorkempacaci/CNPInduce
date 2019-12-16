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
        private static readonly ISet<string> idArgNames = new HashSet<string>() { "a", "b" };
        private static readonly IEnumerable<Signature> idModes = new List<Signature>
        {
            new Signature(("a", ArgumentMode.In), ("b", ArgumentMode.Out)),
            new Signature(("a", ArgumentMode.Out), ("b", ArgumentMode.In)),
            new Signature(("a", ArgumentMode.In), ("b", ArgumentMode.In))
        };
        
        public override ISet<string> ArgumentNames => idArgNames;

        public override string ToString()
        {
            return "id";
        }

        public static Id FromObservation(ObservedProgram obs)
        {
            if (!idArgNames.SetEquals(obs.ArgumentNames) || !idModes.Contains(obs.Signature))
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
