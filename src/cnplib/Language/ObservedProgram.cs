﻿using System;
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

        // private static ISet<string> FindArgNames(IEnumerable<AlphaTuple> ats)
        // {
        //     if (!ats.Any())
        //         throw new Exception("FindArgNames: List of tuples is empty.");
        //     var allKeys = ats.Select(at => at.Terms.Keys);
        //     var first = allKeys.First();
        //     if (allKeys.All(ks => ks.SequenceEqual(first)))
        //     {
        //         var set = new HashSet<string>(first);
        //         return set;
        //     }
        //     else
        //     {
        //         throw new Exception("FindArgNames: Keys in a list of tuples are not the same: " + ats.ToString());
        //     }
        // }
    }
}
