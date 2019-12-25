using System;
using System.Collections.Generic;
using System.Threading;
using CNP.Language;
using CNP.Helper;
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
                        IEnumerable<Program> alternates = AlternateOnFirstHole(open, ref queue.SearchedProgramsCount);
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
        /// <summary>
        /// Takes an open program, finds the first whole, and returns clones of the program where the whole is filled with different alternatives.
        /// </summary>
        private static IEnumerable<Program> AlternateOnFirstHole(Program open, ref int searchCounter)
        {
            List<Program> programs = new List<Program>();
            var fillers = new List<Func<ObservedProgram, IEnumerable<Program>>>
            {
                Id.FromObservation,
                Cons.FromObservation,
                Const.FromObservation,
                FoldR.FromObservation,
                FoldL.FromObservation
            };
            foreach(var filler in fillers)
            {
                Interlocked.Increment(ref searchCounter);
                Program cloneProgram = open.Clone();
                ObservedProgram hole = cloneProgram.FindFirstHole();
                IEnumerable<Program> newSubTrees = filler(hole);
                foreach (Program subTree in newSubTrees)
                {
                    Program newProgram = cloneProgram.CloneAndReplace(hole, subTree, new FreeDictionary());
                    programs.Add(newProgram);
                }
            }
            return programs;
        }
    }
}
