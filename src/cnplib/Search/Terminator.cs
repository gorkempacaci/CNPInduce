using System;
using System.Threading;

namespace CNP.Search
{
  public class Terminator
  {
    CancellationTokenSource cancellation;
    object hungryCountLock = new();
    readonly int maxHungryThreads;
    int currentHungryThreads = 0;

    public Terminator(CancellationTokenSource can, int numberOfMaxHungryThreads)
    {
      cancellation = can;
      maxHungryThreads = numberOfMaxHungryThreads;
    }

    public void ReportHungry()
    {
      lock(hungryCountLock)
      {
        currentHungryThreads++;
        if (maxHungryThreads == currentHungryThreads)
          cancellation.Cancel();
        if (currentHungryThreads > maxHungryThreads)
          throw new ArgumentOutOfRangeException("Too many hungry threads.");
      }
    }
    public void ReportNotHungry()
    {
      lock(hungryCountLock)
      {
        currentHungryThreads--;
        if (currentHungryThreads < 0)
          throw new ArgumentOutOfRangeException("Too few hungry threads.");
      }
    }
  }
}
