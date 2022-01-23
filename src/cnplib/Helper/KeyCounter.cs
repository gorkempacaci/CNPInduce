using System;
using System.Threading;
using CNP.Helper.EagerLinq;

namespace Helper
{
  /// <summary>
  /// Thread-safe if threads use separate keys.
  /// </summary>
  public class KeyCounter
  {   
    int[] counts; 
    public KeyCounter(int numberOfKeys)
    {
      counts = new int[numberOfKeys];
    }

    public void Increase(int key)
    {
      counts[key]++;
    }

    public override string ToString()
    {
      return "{" + string.Join(",", counts) + "}";
    }
  }
}
