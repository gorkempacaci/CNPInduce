using System;
using CNP.Language;
namespace CNP.Search
{
    public interface IProgramSearchReceiver
    {
        /// <summary>
        /// Returns true to indicate the search should stop.
        /// </summary>
        bool FoundNewProgram(ProgramEnvironment p);
        /// <summary>
        /// Notifies that the search is done.
        /// </summary>
        void SearchIsFinished();
    }
}
