using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CNP.Language;

namespace CNP.Search
{
    /// <summary>
    /// The top-level object for synthesis jobs.
    /// </summary>
    public class SynthesisJob : IProgramSearchReceiver
    {
        const int DEFAULT_MAX_HEIGHT = 5;

        int programCountLimit;
        ProgramSearch search;

        ConcurrentQueue<Program> programs = new ConcurrentQueue<Program>();

        public SynthesisJob(ObservedProgram initialObservation, int programCount = int.MaxValue, int maxHeight = DEFAULT_MAX_HEIGHT)
        {
            programCountLimit = programCount;
            search = new ProgramSearch(initialObservation, this, maxHeight);
        }

        /// <summary>
        /// Finds all programs up to depth. Blocks until search is complete. Guarantees that in the returned list, shallower programs come first.
        /// </summary> 
        public IEnumerable<Program> FindAllPrograms()
        {
            search.Start();
            search.WaitUntilDone();
            return programs;
        }

        public bool FoundNewProgram(Program p)
        {
            programs.Enqueue(p);
            return !(programs.Count < programCountLimit);
        }

        public void SearchIsFinished()
        {
            
        }
    }
}
