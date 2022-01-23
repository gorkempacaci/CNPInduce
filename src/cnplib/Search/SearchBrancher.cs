using CNP.Helper;using CNP.Language;using CNP.Helper.EagerLinq;using System;using System.Collections.Concurrent;
using System.Collections.Generic;using System.Threading;
namespace CNP.Search{

  /// <summary>  /// Pops the program search queue, branches and pushes the new branches back, on a loop.  /// TODO: BUG  /// This way of pop/push is not order preserving.  /// Some holes may be quicker to fill than others, so the order they are  /// produced differs. As a result some longer programs may be found earlier than shorter ones.  /// </summary>  public class SearchBrancher  {    private ProgramSearch searchManager;    private ConcurrentQueue<Program> candidates;    private IProgramSearchReceiver resultsReceiver;    private CancellationTokenSource cancellationTokenSource;    private int myid;    private bool haveReportedHungry = false;    private Terminator hungerTracker;    public SearchBrancher(ProgramSearch search, ConcurrentQueue<Program> _candidates, Terminator hTrack, CancellationTokenSource _tokenSource, IProgramSearchReceiver _resultsReceiver, int id)    {      searchManager = search;      candidates = _candidates;      hungerTracker = hTrack;      cancellationTokenSource = _tokenSource;      resultsReceiver = _resultsReceiver;      myid = id;    }    public void ConsumeProduceLoop()
    {
      Program pFromQueue;
      while (true)
      {
        if (cancellationTokenSource.IsCancellationRequested)
          break;
        if (candidates.TryDequeue(out pFromQueue))
        {
          if (haveReportedHungry)
          {
            hungerTracker.ReportNotHungry();
            haveReportedHungry = false;
          }
        }
        else
        {
          if (!haveReportedHungry)
          {
            hungerTracker.ReportHungry();
            haveReportedHungry = true;
          }
          continue;
        }
//        #region DEBUG
//#if DEBUG
//        Interlocked.Increment(ref Debugging.CountDequeued);
//        Debugging.ThreadDequeueCounter.Increase(myid);
//#endif
//        #endregion
        var alternates = AlternateOnFirstHole(pFromQueue, out bool isEmpty);
          foreach (Program ap in alternates)
          {
            if (ap.IsClosed)
            {
              bool shouldStop = resultsReceiver.FoundNewProgram(ap);
              if (shouldStop)
              {
                signalStop();
                break;
              }
            }
            else
            {
              candidates.Enqueue(ap);
//              #region DEBUG
//#if DEBUG
//              Interlocked.Increment(ref Debugging.CountQueued);
//#endif
//              #endregion
            }
          
        }
      }
    }


    private void signalStop()
    {
      cancellationTokenSource.Cancel();
    }    /// <summary>    /// Takes an open program, finds the first whole, and returns clones of the program where the whole is filled with different alternatives.    /// TODO: Optimize the list building into a stream    /// </summary>    private static IEnumerable<Program> AlternateOnFirstHole(Program open, out bool isEmpty)    {      List<Program> programs = new List<Program>();      var fillers = new List<Func<Program, IEnumerable<Program>>>      {                Id.CreateAtFirstHole                ,Cons.CreateAtFirstHole                ,Const.CreateAtFirstHole                ,FoldR.CreateAtFirstHole                ,FoldL.CreateAtFirstHole                ,Proj.CreateAtFirstHole                ,And.CreateAtFirstHole      };      isEmpty = true;      foreach (var filler in fillers)      {        IEnumerable<Program> newPrograms = filler(open); // these may be open or closed at this point
#if DEBUG        //if (newPrograms.Count() > 1000)
        //  throw new ArgumentOutOfRangeException();
#endif        foreach (Program p in newPrograms)        {          programs.Add(p);          isEmpty = false;        }      }      return programs;    }  }}