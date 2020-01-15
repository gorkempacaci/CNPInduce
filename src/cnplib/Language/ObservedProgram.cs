using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CNP.Helper;
using CNP.Helper.EagerLinq;
using Enumerable = System.Linq.Enumerable;

namespace CNP.Language
{

    /// <summary>
    /// An unbound program variable, an observation. Immutable object.
    /// </summary>
    public class ObservedProgram : Program
    {

        public override int Height => 0;

        public readonly IEnumerable<AlphaTuple> Observables;
        public readonly NameModeMap Domains;

        public ObservedProgram(IEnumerable<AlphaTuple> obsv, NameModeMap doms)
        {
            IsClosed = false;
            Observables = obsv;
            Domains = doms;
        }

        /// <summary>
        /// Makes the free argument names in this OP ground by using given names. 
        /// </summary>
        /// TODO: Optimize
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public IEnumerable<ObservedProgram> CloneToGroundDomains(NameModeMap targetDomains)
        {
            IEnumerable<KeyValuePair<ArgumentName,ArgumentMode>> myGroundDomains =
                this.Domains.WhereAndNot(n => n.Key.IsGround(), out IEnumerable<KeyValuePair<ArgumentName,ArgumentMode>> myFreeDomains);
            var targetDomsToMatch = targetDomains.Except(myGroundDomains);
            if (targetDomains.Count() != targetDomsToMatch.Count() + myGroundDomains.Count())
                return Iterators.Empty<ObservedProgram>();
            if (myFreeDomains.Count() != targetDomsToMatch.Count())
                return Iterators.Empty<ObservedProgram>();
            var myFreeIns = myFreeDomains.Where(d => d.Value == ArgumentMode.In);
            var myFreeOuts = myFreeDomains.Where(d => d.Value == ArgumentMode.Out);
            var targetGroundIns = targetDomsToMatch.Where(d => d.Value == ArgumentMode.In);
            var targetGroundOuts = targetDomsToMatch.Where(d => d.Value == ArgumentMode.Out);
            if (myFreeIns.Count() != targetGroundIns.Count() || myFreeOuts.Count() != targetGroundOuts.Count())
                throw new Exception("CloneToGroundDomains: Free In or Out domains on ObservedProgram doesn't match free target domains.");
            var targetInPerms = targetGroundIns.Permutations();
            var targetOutPerms = targetGroundOuts.Permutations();
            var inBindings = targetInPerms.Select(gs => myFreeIns.Zip(gs, (f, g) => new KeyValuePair<Term, Term>(f.Key, g.Key)));
            var outBindings = targetOutPerms.Select(gs => myFreeOuts.Zip(gs, (f, g) => new KeyValuePair<Term, Term>(f.Key, g.Key)));
            var allBindings = inBindings.Cartesian(outBindings, (@is, os) => @is.Concat(os));
            var allObservations = allBindings.Select(o => this.CloneAndReplace(new TermReferenceDictionary(o)) as ObservedProgram);
            return allObservations;
        }

        /// <summary>
        /// Replaces itself if it is the oldComponent.
        /// </summary>
        public override Program CloneAndReplace(TermReferenceDictionary plannedParenthood, ObservedProgram oldComponent=null,
            Program newComponent=null)
        {
            // If this is the oldComponent they're looking for
            if (object.ReferenceEquals(this, oldComponent))
            {
                return newComponent;
            }
            else
            {
                var clonedObservables = Observables.Select(o => o.Clone(plannedParenthood));
                var clonedDomains = Domains.Clone(plannedParenthood);
                return new ObservedProgram(clonedObservables, clonedDomains);
            }
        }

        internal override ObservedProgram FindFirstHole()
        {
            return this;
        }
    }
}
