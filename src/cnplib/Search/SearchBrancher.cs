using CNP.Helper;using CNP.Language;using CNP.Helper.EagerLinq;using System;using System.Collections.Concurrent;using System.Collections.Generic;using System.Threading;using System.Linq;namespace CNP.Search{  /// <summary>  /// Pops the program search queue, branches and pushes the new branches back, on a loop.  /// TODO: BUG  /// This way of pop/push is not order preserving.  /// Some holes may be quicker to fill than others, so the order they are  /// produced differs. As a result some longer programs may be found earlier than shorter ones.  /// </summary>  public class SearchBrancher  {    private ProgramSearch searchManager;    private ConcurrentQueue<ProgramEnvironment> candidates;    private IProgramSearchReceiver resultsReceiver;    private CancellationTokenSource cancellationTokenSource;    private int myid;    //private bool haveReportedHungry = false;    private CountdownEvent countDown;
    private GroundRelation negativeExamples;

    static IEnumerable<Func<ProgramEnvironment, IEnumerable<ProgramEnvironment>>> fillers_and_elem_lib;

    static IEnumerable<Func<ProgramEnvironment, IEnumerable<ProgramEnvironment>>> fillers_all;    static SearchBrancher()
    {
      var elementary = new List<Func<ProgramEnvironment, IEnumerable<ProgramEnvironment>>>()
      {
        Id.CreateAtFirstHole, Cons.CreateAtFirstHole, Const.CreateAtFirstHole
      };
      var library = MathLib.MathLibrary.Select(l => (Func<ProgramEnvironment, IEnumerable<ProgramEnvironment>>)l.CreateAtFirstHole);
      var folds = new[] { FoldL.CreateAtFirstHole, FoldR.CreateAtFirstHole };
      var proj = new[] { Proj.CreateAtFirstHole };
      var and = new[] { And.CreateAtFirstHole };

      fillers_and_elem_lib = elementary.Concat(library).Concat(and);
      fillers_all = elementary.Concat(library).Concat(folds).Concat(proj).Concat(and);
    }    public SearchBrancher(ProgramSearch search, GroundRelation negativeExamples, ConcurrentQueue<ProgramEnvironment> _candidates, CountdownEvent countDownParam, CancellationTokenSource _tokenSource, IProgramSearchReceiver _resultsReceiver,  int id)    {      searchManager = search;      candidates = _candidates;      this.countDown = countDownParam;      cancellationTokenSource = _tokenSource;      resultsReceiver = _resultsReceiver;      myid = id;      this.negativeExamples = negativeExamples.Clone(new CloningContext());    }    public void ConsumeProduceLoop()    {      //SynthesisJob.PreInitialize();      ProgramEnvironment pFromQueue;      while (true)      {        if (cancellationTokenSource.IsCancellationRequested) // || countDown.IsSet)
          return;
        if (candidates.TryDequeue(out pFromQueue))        {
          var alternates = AlternateOnFirstHole(pFromQueue, out bool isEmpty);
          foreach (ProgramEnvironment ap in alternates)
          {
            //if (cancellationTokenSource.IsCancellationRequested)
            //  return;
            if (ap.Root.IsClosed)
            {
              if (IProgram.HasProgramSymmetry(ap.Root, ap))
                continue;
              if (!ExecutionEnvironment.NegativeExamplesFailAsTheyShould(ap, this.negativeExamples))
                continue;
              if (resultsReceiver.FoundNewSolution(ap)) // if accepted
              {
                cancellationTokenSource.Cancel();
                return;
              }
            }
            else
            {
//              #region DEBUG
//#if DEBUG
//              var hole = ap.Root.FindHole();
//              if (!hole.Observations.All(o => o.IsAllINArgumentsGroundForFirstTuple()))
//                throw new Exception("Not all INS are ground.");
//#endif
//              #endregion
              candidates.Enqueue(ap);
              //countDown.AddCount();
            }
          }
          //countDown.Signal(); // signal -1 to the countdown for the program dequeued.
        }
      }    }    /// <summary>    /// Takes an open program, finds the first whole, and returns clones of the program where the whole is filled with different alternatives.    /// TODO: Optimize the list building into a stream    /// </summary>    private static IEnumerable<ProgramEnvironment> AlternateOnFirstHole(ProgramEnvironment openOrig, out bool isEmpty)    {      List<ProgramEnvironment> programs = new();      var fillers = openOrig.Root.FindHole().Constraints == ObservedProgram.Constraint.OnlyAndElemLib        ? fillers_and_elem_lib        : fillers_all;       isEmpty = true;      foreach (var filler in fillers)      {        var open = openOrig.Clone();        IEnumerable<ProgramEnvironment> newPrograms = filler(open); // these may be open or closed at this point        foreach (var p in newPrograms)        {          programs.Add(p);          isEmpty = false;        }      }      return programs;    }  }}