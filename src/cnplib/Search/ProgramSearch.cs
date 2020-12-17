using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using CNP.Helper;
using CNP.Language;
namespace CNP.Search
{
  public class ProgramSearch
  {
    readonly int threadCount;
    readonly int maxHeightForPrograms;
    int busyThreadCount = 0;
    object busyThreadCountMonitor = new object();
    public int SearchedProgramsCount = 0;
    ConcurrentQueue<Program> searchQueue = new ConcurrentQueue<Program>();
    List<SearchBrancher> threadObjects;
    List<Thread> systemThreads;
    IProgramSearchReceiver searchReceiver;

    public ProgramSearch(ObservedProgram initialHole, IProgramSearchReceiver receiver, ThreadCount tCount = default)
    {
      searchReceiver = receiver;
      threadCount = tCount.GetNumberOfThreads();
      maxHeightForPrograms = initialHole.DTL;
      searchQueue.Enqueue(initialHole);
    }
    public void WaitUntilDone()
    {
      foreach (Thread t in systemThreads)
      {
        try
        {
          t.Join();
        }
        catch (Exception)
        {
          throw;
        }
      }
    }
    public void Start()
    {
      threadObjects = new List<SearchBrancher>(threadCount);
      systemThreads = new List<Thread>(threadCount);
      for (int i = 0; i < threadCount; i++)
      {
        SearchBrancher pst = new SearchBrancher(this);
        threadObjects.Add(pst);
        Thread t = new Thread(pst.Start);
        systemThreads.Add(t);
        t.Start();
      }
    }

    /// <summary>
    /// Returns the next program in the top of the queue. Outputs a program that is not closed.
    /// True if Take() was successfull. False if there is nothing in the queue.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the search is terminated throws InvalidOperationException.</exception>
    public bool TryTake(out Program program, out Action<IEnumerable<Program>> queueCallback)
    {
      lock (busyThreadCountMonitor)
      {
        if (searchReceiver == null)
        {
          throw new InvalidOperationException("Search is terminated.");
        }
        else if (busyThreadCount == 0 && searchQueue.Count == 0)
        {
          throw new InvalidOperationException("Search is finished.");
        }
        else if (searchQueue.TryDequeue(out program))
        {
          busyThreadCount++;
          queueCallback = new JustOnce<IEnumerable<Program>>(queue).Invoke;
          return true;
        }
        else
        {
          program = null;
          queueCallback = null;
          return false;
        }
      }
    }
    void queue(IEnumerable<Program> ps)
    {
      lock (busyThreadCountMonitor)
      {
        busyThreadCount--;
      }
      if (searchReceiver == null)
      {
        return;
      }
      foreach (Program p in ps)
      {
        if (p.IsClosed)
        {
          searchReceiver?.FoundNewProgram(p);
        }
        else
        {
#if DEBUG // do this check only in debug mode as GetHeight is recursive.
          if (p.GetHeight() > maxHeightForPrograms)
            throw new InvalidOperationException("Invalid new search node. Program's height is greater than the max search height (" + maxHeightForPrograms + "). \nProgram: " + p.ToString());
#endif
          searchQueue.Enqueue(p);
        }
      }
    }
  }
}
