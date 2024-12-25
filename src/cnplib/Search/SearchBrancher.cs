using CNP.Helper;
using CNP.Language;
using CNP.Helper.EagerLinq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

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

    private ProgramSearch searchManager;
    private ConcurrentQueue<ProgramEnvironment> candidates;
    private IProgramSearchReceiver resultsReceiver;
    private CancellationTokenSource cancellationTokenSource;
    private int myid;
    //private bool haveReportedHungry = false;
    private CountdownEvent countDown;
    private GroundRelation negativeExamples;

    static IEnumerable<Func<ProgramEnvironment, IEnumerable<ProgramEnvironment>>> fillers_and_elem_lib;

    static IEnumerable<Func<ProgramEnvironment, IEnumerable<ProgramEnvironment>>> fillers_all;

    static SearchBrancher()
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
    }

    public SearchBrancher(ProgramSearch search, GroundRelation negativeExamples, ConcurrentQueue<ProgramEnvironment> _candidates, CountdownEvent countDownParam, CancellationTokenSource _tokenSource, IProgramSearchReceiver _resultsReceiver, int id)
    {
      searchManager = search;
      candidates = _candidates;
      this.countDown = countDownParam;
      cancellationTokenSource = _tokenSource;
      resultsReceiver = _resultsReceiver;
      myid = id;
      this.negativeExamples = negativeExamples.Clone(new CloningContext());
    }

    public void ConsumeProduceLoop()
    {
      //SynthesisJob.PreInitialize();
      ProgramEnvironment pFromQueue;
      while (true)
      {
        if (cancellationTokenSource.IsCancellationRequested) // || countDown.IsSet)
          return;
        if (candidates.TryDequeue(out pFromQueue))
        {
          var alternates = AlternateOnFirstHole(pFromQueue, out bool _);
          // Stage 1
          foreach (ProgramEnvironment ap in alternates)
          {
            switch (CheckSuccess(ap))
            {
              case SearchAction.Skip: continue;
              case SearchAction.Done: cancellationTokenSource.Cancel(); return;
              case SearchAction.Expand:
                if (ap.Root.GetHeight() < 2)
                {
                  var newGrandchildCandidates = AlternateOnFirstHole(ap, out bool _);
                  foreach (var ap2 in newGrandchildCandidates)
                  {
                    switch (CheckSuccess(ap2))
                    {
                      case SearchAction.Skip: continue;
                      case SearchAction.Done: cancellationTokenSource.Cancel(); return;
                      case SearchAction.Expand:
                        candidates.Enqueue(ap2);
                        break;
                    }
                  }
                } else candidates.Enqueue(ap);
                break;
            }
          }

        }
      }
    }

    enum SearchAction { Done, Skip, Expand };
    private SearchAction CheckSuccess(ProgramEnvironment pe)
    {
      if (pe.Root.IsClosed)
      {
        if (IProgram.HasProgramSymmetry(pe.Root, pe))
          return SearchAction.Skip;
        if (!ExecutionEnvironment.NegativeExamplesFailAsTheyShould(pe, this.negativeExamples))
          return SearchAction.Skip;
        if (resultsReceiver.FoundNewSolution(pe)) // if accepted
        {
          cancellationTokenSource.Cancel();
          return SearchAction.Done;
        }
      }
      return SearchAction.Expand;
    }

    /// <summary>
    /// Takes an open program, finds the first whole, and returns clones of the program where the whole is filled with different alternatives.
    /// TODO: Optimize the list building into a stream
    /// </summary>
    private static IEnumerable<ProgramEnvironment> AlternateOnFirstHole(ProgramEnvironment openOrig, out bool isEmpty)
    {
      List<ProgramEnvironment> programs = new();

      var fillers = openOrig.Root.FindHole().Constraints == ObservedProgram.Constraint.OnlyAndElemLib
        ? fillers_and_elem_lib
        : fillers_all;
      isEmpty = true;
      foreach (var filler in fillers)
      {
        var open = openOrig.Clone();
        IEnumerable<ProgramEnvironment> newPrograms = filler(open); // these may be open or closed at this point
        foreach (var p in newPrograms)
        {
          programs.Add(p);
          isEmpty = false;
        }
      }
      return programs;
    }
  }
}