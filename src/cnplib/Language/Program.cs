using System;
using System.Collections;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Helper.EagerLinq;


namespace CNP.Language
{

    public abstract class Program
    {
        public bool IsClosed { get; protected set; }

        /// <summary>
        /// Returns a deep copy of this program.
        /// </summary>
        /// <returns></returns>
        public Program Clone()
        {
            return CloneAndReplace(null, null, new FreeDictionary());
        }

        /// <summary>
        /// Makes a deep copy of this program, where the oldComponent is replaced by newComponent, matched by reference. If oldComponent is this, then it just returns the newComponent.
        /// </summary>
        /// <param name="oldComponent"></param>
        /// <param name="newComponent"></param>
        /// <returns></returns>
        public abstract Program CloneAndReplace(ObservedProgram oldComponent, Program newComponent, FreeDictionary plannedParenthood);
        /// <summary>
        /// Returns the first non-ground program (ObservedProgram) in the subtree, first as in in-order, LNR search.
        /// If there is no hole, returns null.
        /// </summary>
        internal abstract ObservedProgram FindFirstHole();

        public abstract ISet<string> ArgumentNames { get; }

        public abstract int Height { get; }
    }

}
