using System;
using System.Collections.Generic;
using System.Threading;
using CNP.Language;
using CNP.Helper;
namespace CNP.Search
{
  /// <summary>
  /// Pops the program search queue, branches and pushes the new branches back, on a loop.
  /// TODO: BUG
  /// This way of pop/push is not order preserving.
  /// Some holes may be quicker to fill than others, so the order they are
  /// produced differs. As a result some longer programs may be found earlier than shorter ones.
  /// </summary>
  public class SearchBrancher
  {
    private ProgramSearch queue;
    private bool stopRequested = false;
    public void Stop()
    {
      this.stopRequested = true;
    }
    public SearchBrancher(ProgramSearch ls)
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
            // if there are no alternatives produced, the open program disappears from search space.
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
      var fillers = new List<Func<Program, IEnumerable<Program>>>
      {
                Id.CreateAtFirstHole,
                Cons.CreateAtFirstHole,
                Const.CreateAtFirstHole,
                FoldR.CreateAtFirstHole,
                FoldL.CreateAtFirstHole
      };
      foreach (var filler in fillers)
      {
        Interlocked.Increment(ref searchCounter);
        IEnumerable<Program> newPrograms = filler(open); // these may be open or closed at this point
        foreach (Program p in newPrograms)
        {
          programs.Add(p);
        }
      }
      return programs;
    }
  }
}