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

        public override int Height => 0;

        public readonly IEnumerable<AlphaTuple> Observables;
        public readonly ProgramType ProgramType;

        public ObservedProgram(IEnumerable<AlphaTuple> obsv, ProgramType s)
        {
            IsClosed = false;
            Observables = obsv;
            ProgramType = s;
        }
        /// <summary>
        /// Replaces itself if it is the oldComponent.
        /// </summary>
        public override Program CloneAndReplace(ObservedProgram oldComponent, Program newComponent, FreeDictionary plannedParenthood)
        {
            // If this is the oldComponent they're looking for
            if (object.ReferenceEquals(this, oldComponent))
            {
                return newComponent;
            }
            else
            {
                var clonedObservables = Observables.Select(o => o.Clone(plannedParenthood));
                return new ObservedProgram(clonedObservables, ProgramType);
            }
        }
        internal override ObservedProgram FindFirstHole()
        {
            return this;
        }
    }
}
