using System;
namespace CNP.Language
{
    /// <summary>
    /// An immediate context a Free can appear in. Specifically AlphaTuples and List terms are potential Free contexts. 
    /// </summary>
    public interface IFreeContext
    {
        /// <summary>
        /// Replace the Free with a given term. Does the occurs check.
        /// </summary>
        void ReplaceAllInstances(Free f, Term t);
    }
}
