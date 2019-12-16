using System;
using CNP.Language;
namespace CNP.Search
{
    public interface IProgramSearchReceiver
    {
        /// <summary>
        /// Returns true to indicate the search should stop.
        /// </summary>
        bool FoundNewProgram(Program p);
        /// <summary>
        /// Notifies that the search is done.
        /// </summary>
        void SearchIsFinished();
    }
}
