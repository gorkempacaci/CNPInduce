using System;

namespace CNP.Search
{
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