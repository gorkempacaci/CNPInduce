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
    Thread[] systemThreads;
    IProgramSearchReceiver searchReceiver;
    GroundRelation negativeExamples;

    public ProgramSearch(ProgramEnvironment initialProgram, GroundRelation negativeExamples, IProgramSearchReceiver receiver, ThreadCount tCount = default)
    {
      searchReceiver = receiver;
      ThreadCount = tCount.GetNumberOfThreads();
      searchQueue.Enqueue(initialProgram);
      this.negativeExamples = negativeExamples;
    }
    
    public void StartThreadsAndWait()
    {
      threadObjects = new List<SearchBrancher>(ThreadCount);
      systemThreads = new Thread[ThreadCount];
      //Terminator hungerTracker = new Terminator(CancellationSource, ThreadCount);
      CountdownEvent cde = new CountdownEvent(1);
      for (int i = 0; i < ThreadCount; i++)
      {
        SearchBrancher pst = new SearchBrancher(this, negativeExamples, searchQueue, cde, CancellationSource, searchReceiver, i);
        threadObjects.Add(pst);
        systemThreads[i] = new Thread(pst.ConsumeProduceLoop);
        systemThreads[i].Name = "CombInduce_Worker_" + i.ToString();
        systemThreads[i].Priority = ThreadPriority.Highest;
        systemThreads[i].Start();
      }
      for (int i = 0; i < ThreadCount; i++)
      {
        systemThreads[i].Join();
      }
    }

  }
}
