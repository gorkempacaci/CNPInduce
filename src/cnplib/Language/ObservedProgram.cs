using System;
using System.Collections;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{

    /// <summary>
    /// An unbound program variable, an observation. Immutable object.
    /// </summary>
    public class ObservedProgram : Program
    {
        private ISet<string> _argNames;
        public override ISet<string> ArgumentNames => _argNames;

        public override int Height => 0;

        public readonly IEnumerable<AlphaTuple> Observables;
        public readonly Signature Signature;

        public ObservedProgram(IEnumerable<AlphaTuple> obsv, Signature s)
        {
            IsClosed = false;
            Observables = obsv;
            _argNames = FindArgNames(obsv);
            Signature = s;
        }
        /// <summary>
        /// Replaces itself if it is the oldComponent.
        /// </summary>
        public override Program CloneAndGrind(ObservedProgram oldComponent, Program newComponent, FreeDictionary plannedParenthood)
        {
            // If this is the oldComponent they're looking for
            if (object.ReferenceEquals(this, oldComponent))
            {
                return newComponent;
            }
            else
            {
                var clonedObservables = Observables.Select(o => o.Clone(plannedParenthood));
                return new ObservedProgram(clonedObservables, Signature);
            }
        }
        internal override ObservedProgram FindFirstHole()
        {
            return this;
        }

        private static ISet<string> FindArgNames(IEnumerable<AlphaTuple> ats)
        {
            if (ats.Count() == 0)
                throw new Exception("ArgumentNamesFrom: List of tuples is empty.");
            var allKeys = ats.Select(at => at.Terms.Keys);
            var first = allKeys.First();
            if (allKeys.All(ks => ks.SequenceEqual(first)))
            {
                var set = new HashSet<string>(first);
                return set;
            }
            else
            {
                throw new Exception("ArgumentNamesFrom: Keys in a list of tuples are not the same: " + ats.ToString());
            }
        }
    }
}
