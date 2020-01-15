using System;
using System.Collections.Generic;
using CNP.Helper;

namespace CNP.Language
{
    /// <summary>
    /// Elementary programs are always immutable.
    /// </summary>
    public abstract class ElementaryProgram : Program
    {
        public ElementaryProgram()
        {
            IsClosed = true;
        }
        public override Program CloneAndReplace(TermReferenceDictionary plannedParenthood, ObservedProgram oldComponent,
            Program newComponent)
        {
            return this;
        }
        internal override ObservedProgram FindFirstHole()
        {
            return null;
        }
        public override int Height { get => 0; }
    }
}
