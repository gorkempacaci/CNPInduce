using System;
using System.Collections.Generic;
using System.Threading;
using CNP.Language;
namespace CNP.Search
{
    /// <summary>
    /// BUG
    /// This way of pop/push is not order preserving.
    /// Some holes may be quicker to fill than others, so the order they are
    /// produced differs.
    /// </summary>
    public class ProgramSearchThread
    {
        private ProgramSearch queue;
        private bool stopRequested = false;
        public void Stop()
        {
            this.stopRequested = true;
        }
        public ProgramSearchThread(ProgramSearch ls)
        {
            queue = ls;
        }
        public void Start()
        {
            try
            {
                while (!stopRequested)
                {
                    if (queue.TryTake(out Program open, out Action<IEnumerable<Program>> give))
                    {
                        IEnumerable<Program> alternates = Semantics.AlternateOnFirstHole(open, ref queue.SearchedProgramsCount);
                        give(alternates);
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
            }
            catch (InvalidOperationException)
            {
                Stop();
            }
        }
    }
}
