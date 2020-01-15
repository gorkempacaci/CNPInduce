using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CNP.Helper;
using CNP.Helper.EagerLinq;


namespace CNP.Language
{

    public abstract class Program
    {
        /// <summary>
        /// true only if a program tree does not have any program variables (instances of ObservedProgram) in it.
        /// </summary>
        public bool IsClosed { get; protected set; }

        /// <summary>
        /// Returns a deep copy of this program.
        /// </summary>
        /// <returns></returns>
        public Program Clone()
        {
            return CloneAndReplace(new TermReferenceDictionary(), null, null);
        }

        /// <summary>
        /// Makes a deep copy of this program, where the oldComponent is replaced by newComponent, matched by reference. If oldComponent is this, then it just returns the newComponent.
        /// </summary>
        /// <param name="plannedParenthood"></param>
        /// <param name="oldComponent"></param>
        /// <param name="newComponent"></param>
        /// <returns></returns>
        public abstract Program CloneAndReplace(TermReferenceDictionary plannedParenthood, ObservedProgram oldComponent=null,
            Program newComponent=null);
        /// <summary>
        /// Returns the first non-ground program (ObservedProgram) in the subtree, first as in in-order, LNR search.
        /// If there is no hole, returns null.
        /// </summary>
        internal abstract ObservedProgram FindFirstHole();

        public abstract int Height { get; }
    }

}
