using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CNP.Search
{
  public class SwitchingQueue<T>
  {
    ConcurrentQueue<T> removingQueue = new();
    ConcurrentQueue<T> addingQueue = new();
    object switchLock = new();

    public bool TryTake(out T taken)
    {  
      lock (switchLock)
      {
        if (removingQueue.TryDequeue(out taken))
        {
          return true;
        }
        else
        {
          if (removingQueue.IsEmpty)
          {
            //addingQueue.CompleteAdding();
            removingQueue = addingQueue;
            addingQueue = new();
            return removingQueue.TryDequeue(out taken);
          }
          return false;
        }
      }
    }

    public void Add(T obj)
    {
      lock (switchLock)
      {
        addingQueue.Enqueue(obj);
      }
    }

    public bool IsEmpty
    {
      get
      {
        lock (switchLock)
        {
          return removingQueue.IsEmpty && addingQueue.IsEmpty;
        }
      }
    }
  }
}
