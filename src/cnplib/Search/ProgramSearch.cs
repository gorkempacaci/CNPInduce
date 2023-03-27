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
        t.Join();
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

  }
}
