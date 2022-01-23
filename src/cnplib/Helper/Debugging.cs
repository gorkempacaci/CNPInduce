using CNP.Language;
using Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNP.Helper
{
  public class Debugging
  {
    
    public class SearchTreeStats
    {
      public IEnumerable<(string, int)> QualifiersAndCounts;
      public IReadOnlyDictionary<string, (int, object)> LogObjects => logObjects;
      public int CountQueued => Debugging.CountQueued;
      public int CountDequeued => Debugging.CountDequeued;
      public KeyCounter ThreadDequeueCounter => Debugging.ThreadDequeueCounter;
    }

    public static SearchTreeStats ProduceSearchTreeStats(IEnumerable<Program> programs)
    {
      Dictionary<string, List<Program>> categorizedPrograms = new();
      foreach(Program p in programs)
      {
        if (!categorizedPrograms.TryGetValue(p.GetTreeQualifier(), out List<Program> bag))
        {
          bag = new();
          categorizedPrograms.Add(p.GetTreeQualifier(), bag);
        }
        bag.Add(p);
      }
      SearchTreeStats stats = new();
            // Qualifiers and Counts
            stats.QualifiersAndCounts = categorizedPrograms.Select(p => (p.Key, p.Value.Count())).OrderBy(e => e.Item2);
      return stats;
    }

    private static object _logObjectLock = new();
    private static Dictionary<string, (int, object)> logObjects = new();
    public static int CountQueued = 0;
    public static int CountDequeued = 0;
    public static KeyCounter ThreadDequeueCounter = new KeyCounter(32);

    public static void LogObjectWithMax(string key, int value, object o)
    {
      lock(_logObjectLock)
      {
        if (logObjects.TryGetValue(key, out (int, object) pair))
        {
          if (pair.Item1 < value)
            return;
        }
        logObjects[key] = (value, o);
      }
    }
  }
}
