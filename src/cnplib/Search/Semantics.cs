using System;
using System.Collections.Generic;
using System.Threading;
using CNP.Language;
using CNP.Helper;
namespace CNP.Search
{
    public class Semantics
    {
        /// <summary>
        /// Takes an open program, finds the first whole, and returns clones of the program where the whole is filled with different alternatives.
        /// </summary>
        public static IEnumerable<Program> AlternateOnFirstHole(Program open, ref int searchCounter)
        {
            List<Program> programs = new List<Program>(5);
            var fillers = new List<Func<ObservedProgram, Program>>
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
                Program newSubTree = filler(hole);
                if (newSubTree != null)
                {
                    Program newProgram = cloneProgram.CloneAndGrind(hole, newSubTree, new FreeDictionary());
                    programs.Add(newProgram);
                }
            }
            return programs;
        }
    }
}   
    