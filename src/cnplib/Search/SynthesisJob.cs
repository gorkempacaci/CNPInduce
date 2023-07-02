using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CNP.Helper;
using CNP.Language;

namespace CNP.Search
{
  /// <summary>
  /// The top-level object for synthesis jobs.
  /// </summary>
  // TODO: Add job constants.
  public class SynthesisJob : IProgramSearchReceiver
  {
    ProgramSearch search;
    SearchOptions searchOptions;
    Action<ProgramEnvironment> solutionObserver = null;
    ConcurrentQueue<ProgramEnvironment> programs = new ConcurrentQueue<ProgramEnvironment>();
    ConcurrentQueue<ProgramEnvironment> solutions = new ConcurrentQueue<ProgramEnvironment>();

    //public static void PreInitialize()
    //{
    //  var foldval = Fold.Valences.FoldModesByModeNumber.ToString();
    //  var andval = And.AndValences.ToString();
    //}

    /// <param name="tuples">Tuples of the program observation.</param>
    /// <param name="valence">Valence of the program</param>
    /// <param name="maxTreeDepth">Depth=1 only gives elementary programs. Depth=2 gives programs like foldr(cons, id), and so on. </param>
    public SynthesisJob(ProgramEnvironment env, GroundRelation negExamples, ThreadCount tCount = default, SearchOptions sOpt = SearchOptions.FindOnlyFirstProgram)
    {
      search = new ProgramSearch(env, negExamples, this, tCount);
      searchOptions = sOpt;
    }



    /// <summary>
    /// Finds all programs up to depth with given search options. Blocks until search is complete. Guarantees that in the returned list, shallower programs come first.
    /// </summary> 
    public IEnumerable<ProgramEnvironment> FindPrograms(Action<ProgramEnvironment> SolutionObserver = null)
    {
      this.solutionObserver = SolutionObserver;
      search.StartThreadsAndWait();
      return solutions;
    }

    public bool FoundNewSolution(ProgramEnvironment p)
    {
      solutionObserver?.Invoke(p);
      solutions.Enqueue(p);
      if (searchOptions == SearchOptions.FindOnlyFirstProgram)
        return true;
      else return false; // false means do not stop searching
    }
  }

  public enum SearchOptions
  {
    FindOnlyFirstProgram,
    FindAllPrograms
  }
}