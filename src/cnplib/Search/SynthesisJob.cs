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
    ConcurrentQueue<ProgramEnvironment> programs = new ConcurrentQueue<ProgramEnvironment>();

    public static void PreInitialize()
    {
      var foldlval = FoldL.FoldLValences.FoldModesByModeNumber.ToString();
      var foldrval = FoldR.FoldRValences.FoldModesByModeNumber.ToString();
      var andval = And.AndValences.ToString();
    }

    /// <param name="tuples">Tuples of the program observation.</param>
    /// <param name="valence">Valence of the program</param>
    /// <param name="maxTreeDepth">Depth=1 only gives elementary programs. Depth=2 gives programs like foldr(cons, id), and so on. </param>
    public SynthesisJob(ProgramEnvironment env, ThreadCount tCount = default, SearchOptions sOpt = SearchOptions.FindOnlyFirstProgram)
    {
      search = new ProgramSearch(env, this, tCount);
      searchOptions = sOpt;
    }

    /// <summary>
    /// Finds all programs up to depth with given search options. Blocks until search is complete. Guarantees that in the returned list, shallower programs come first.
    /// </summary> 
    public IEnumerable<ProgramEnvironment> FindPrograms()
    {
      search.StartThreads();
      search.WaitUntilDone();
      //var set = programs.ToHashSet(); // don't repeat same program twice. TODO: Very inefficient to convert to set after the fact. This should be handled while the search is populating the list.
      return programs;
    }

    public bool FoundNewProgram(ProgramEnvironment p)
    {
      programs.Enqueue(p);
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

  public struct ThreadCount
  {
    int numberOfThreads;
    /// <summary>
    /// If n=0, it means one thread per logical processor.
    /// </summary>
    /// <param name="n"></param>
    public ThreadCount(int n)
    {
      numberOfThreads = n;
    }
    public int GetNumberOfThreads()
    {
      if (numberOfThreads == 0)
        return Environment.ProcessorCount;
      else return numberOfThreads;
    }
  }
}