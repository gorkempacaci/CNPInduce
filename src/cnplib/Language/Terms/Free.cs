using System;
using System.Collections.Generic;
using CNP.Helper;

namespace CNP.Language
{
    public interface IFreeContainer
    {
        void SubstituteFreeInPlace(Free oldTerm, Term newTerm);
    }
    public class Free : Term
    {
        /// <summary>
        /// For printing variable ids
        /// </summary>
        private static int idCounter;
        private readonly int id = idCounter++;

        public bool IsFree => true;

        private HashSet<IFreeContainer> immediateContainers = new HashSet<IFreeContainer>(ReferenceEqualityComparer<IFreeContainer>.Instance);

        /// <summary>
        /// Parameter is an immediate container of this Free. It could be a list term, for example container=[1,2,Free] or
        /// an AlphaTuple.
        /// </summary>
        public void RegisterContainer(IFreeContainer container)
        {
            if (!immediateContainers.Contains(container))
            {
                this.immediateContainers.Add(container);
            }
        }

        public void SubstituteInContainers(Term newTerm)
        {
            foreach (IFreeContainer c in immediateContainers)
            {
                c.SubstituteFreeInPlace(this, newTerm);
            }
        }

        public override bool IsGround()
        {
            return false;
        }

        public override Term Clone(FreeDictionary plannedParenthood)
        {
            Term newMe;
            if (!plannedParenthood.TryGetValue(this, out newMe))
            {
                newMe = new Free();
                plannedParenthood.Add(this, newMe);
            }
            return newMe;
        }

        public override bool Contains(Free other) => false;

        public override string ToString()
        {
            return "_" + id.ToString();
        }
    }
}
