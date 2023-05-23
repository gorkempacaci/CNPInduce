using CNP.Helper;using CNP.Language;using CNP.Helper.EagerLinq;using System;using System.Collections.Concurrent;using System.Collections.Generic;using System.Threading;using System.Linq;namespace CNP.Search{  /// <summary>  /// Pops the program search queue, branches and pushes the new branches back, on a loop.  /// TODO: BUG  /// This way of pop/push is not order preserving.  /// Some holes may be quicker to fill than others, so the order they are  /// produced differs. As a result some longer programs may be found earlier than shorter ones.  /// </summary>  public class SearchBrancher  {    private ProgramSearch searchManager;    private ConcurrentQueue<ProgramEnvironment> candidates;    private IProgramSearchReceiver resultsReceiver;    private CancellationTokenSource cancellationTokenSource;    private int myid;    //private bool haveReportedHungry = false;    private CountdownEvent countDown;

    static Func<ProgramEnvironment, IEnumerable<ProgramEnvironment>>[] fillers_and_elem_lib = new[] {
      Plus.CreateAtFirstHole      ,Id.CreateAtFirstHole      ,Cons.CreateAtFirstHole      ,Const.CreateAtFirstHole      ,And.CreateAtFirstHole
    };

    static Func<ProgramEnvironment, IEnumerable<ProgramEnvironment>>[] fillers_all = new[]
    {      Plus.CreateAtFirstHole      ,Id.CreateAtFirstHole      ,Cons.CreateAtFirstHole      ,Const.CreateAtFirstHole      ,FoldL.CreateAtFirstHole      ,FoldR.CreateAtFirstHole      ,Proj.CreateAtFirstHole      ,And.CreateAtFirstHole    };    public SearchBrancher(ProgramSearch search, ConcurrentQueue<ProgramEnvironment> _candidates, CountdownEvent countDownParam, CancellationTokenSource _tokenSource, IProgramSearchReceiver _resultsReceiver, int id)    {      searchManager = search;      candidates = _candidates;      this.countDown = countDownParam;      cancellationTokenSource = _tokenSource;      resultsReceiver = _resultsReceiver;      myid = id;    }    public void ConsumeProduceLoop()    {      ProgramEnvironment pFromQueue;      while (true)      {        if (cancellationTokenSource.IsCancellationRequested)          break;        if (candidates.TryDequeue(out pFromQueue))        {
          // don't signal cde yet, otherwise it'll hit 0 for the first program immediately.
        }        else        {          continue; // try dequeueing again, shouldn't happen often
        }        var alternates = AlternateOnFirstHole(pFromQueue, out bool isEmpty);
        foreach (ProgramEnvironment ap in alternates)
        {
          if (ap.Root.IsClosed)
          {
            bool shouldStop = resultsReceiver.FoundNewSolution(ap);
            if (shouldStop)
            {
              cancellationTokenSource.Cancel();
              break;
            }
          }
          else
          {
            // badly-ground search paths don't live on
#if DEBUG
            var hole = ap.Root.FindHole();
            if (!hole.Observations.All(o => o.IsAllINArgumentsGroundForFirstTuple()))
              throw new Exception("Not all INS are ground.");
#endif
            candidates.Enqueue(ap);
            countDown.AddCount();
          }
        }
        countDown.Signal(); // signal -1 to the countdown for the program dequeued.
      }      //try
      //{
      //  countDown.Wait(cancellationTokenSource.Token);
      //}      //catch(OperationCanceledException)      //{
      //  // done
      //}      //finally      //{
      //  Thread.EndThreadAffinity();
      //}    }    /// <summary>    /// Takes an open program, finds the first whole, and returns clones of the program where the whole is filled with different alternatives.    /// TODO: Optimize the list building into a stream    /// </summary>    private static IEnumerable<ProgramEnvironment> AlternateOnFirstHole(ProgramEnvironment openOrig, out bool isEmpty)    {      List<ProgramEnvironment> programs = new();      var fillers = openOrig.Root.FindHole().Constraints == ObservedProgram.Constraint.OnlyAndElemLib        ? fillers_and_elem_lib        : fillers_all;       isEmpty = true;      foreach (var filler in fillers)      {        var open = openOrig.Clone();        IEnumerable<ProgramEnvironment> newPrograms = filler(open); // these may be open or closed at this point        foreach (var p in newPrograms)        {          programs.Add(p);          isEmpty = false;        }      }      return programs;    }  }}