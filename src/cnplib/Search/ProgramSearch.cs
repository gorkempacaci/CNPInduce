using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using CNP.Helper;
using CNP.Language;
using CNP.Helper.EagerLinq;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace CNP.Search
{
  public class ProgramSearch
  {
    //public int BusyThreadCount = 0;
    public readonly int ThreadCount;
    ConcurrentQueue<ProgramEnvironment> searchQueue = new();
    object searchQueueDequeueLock = new();
    CancellationTokenSource CancellationSource = new();
    List<SearchBrancher> threadObjects;
    List<Thread> systemThreads;
    IProgramSearchReceiver searchReceiver;

    public ProgramSearch(ProgramEnvironment initialProgram, IProgramSearchReceiver receiver, ThreadCount tCount = default)
    {
      searchReceiver = receiver;
      ThreadCount = tCount.GetNumberOfThreads();
      searchQueue.Enqueue(initialProgram);
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
    public void StartThreads()
    {
      threadObjects = new List<SearchBrancher>(ThreadCount);
      systemThreads = new List<Thread>(ThreadCount);
      Terminator hungerTracker = new Terminator(CancellationSource, ThreadCount);
      for (int i = 0; i < ThreadCount; i++)
      {
        SearchBrancher pst = new SearchBrancher(this, searchQueue, hungerTracker, CancellationSource, searchReceiver, i);
        threadObjects.Add(pst);
        Thread t = new Thread(pst.ConsumeProduceLoop);
        t.Priority = ThreadPriority.Highest;
        t.Name = "Synth" + i;
        systemThreads.Add(t);
        //Interlocked.Increment(ref BusyThreadCount);
        t.Start();
      }
    }

    /// <summary>
    /// Returns the next program in the top of the queue. Outputs a program that is not closed.
    /// True if Take() was successfull. False if there is nothing in the queue.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the search is terminated throws InvalidOperationException.</exception>
//    public bool TryTake___(out Program program, out Action<IEnumerable<Program>> queueCallback, out bool searchCompleted)
//    {
//      searchCompleted = false;
//      lock (busyThreadCountMonitor)
//      {
//        if (searchReceiver == null)
//        {
//          //program = null;
//          //queueCallback = null;
//          //searchCompleted = true;
//          //return false;
//          throw new InvalidOperationException("Search is terminated.");
//        }
//        else if (busyThreadCount == 0 && searchQueue.Count == 0)
//        {
//          program = null;
//          queueCallback = null;
//          searchCompleted = true;
//          return false;
//          //throw new InvalidOperationException("Search is finished.");
//        }
//        else if (searchQueue.TryTake(out program))
//        {
//          busyThreadCount++;
//          queueCallback = new JustOnce<IEnumerable<Program>>(queue).Invoke;
//#if DEBUG
//          Interlocked.Increment(ref Debugging.CountDequeued);
//          Debugging.ThreadDequeueCounter.Increase(Thread.CurrentThread.ManagedThreadId);
//#endif
//          return true;
//        }
//        else
//        {
//          program = null;
//          queueCallback = null;
//          return false;
//        }
//      }
//    }

  }
}
