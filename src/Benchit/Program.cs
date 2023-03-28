
using System.Text.Json;
using CNP.Search;
using CNP.Parsing;
using CNP.Language;
using CNP.Helper;
using System.Text;
using CNP;
using System.Diagnostics.CodeAnalysis;

namespace Benchit
{


  public class Program
  {
    const int WAIT_BETWEEN_RUNS_MS = 200;
    const int WAIT_BETWEEN_TASKS_MS = 2000;

    [RequiresAssemblyFiles()]
    public static int Main(string[] args)
    {
      if (args.Length == 0 || args[0] == "--help")
      {
        Console.WriteLine("Usage: benchit FILENAME THREAD_COUNTS_ARRAY REPEATS");
        Console.WriteLine("Example: benchit benchmarks.json [1,2,3,4] 3");
        Console.WriteLine("Example json file:");
        Console.WriteLine(BenchmarkFile.GetExampleBenchmarksFileString());
        return 0;
      }
      // PRINTING CNP VERSION
      string cnpTimeString = System.IO.File.GetCreationTime(typeof(IProgram).Assembly.Location).ToString();
      Console.WriteLine("Using CNP: " + cnpTimeString);
      Console.WriteLine("Preinitializing. ");
      SynthesisJob.PreInitialize();
      Thread.Sleep(1000);
      // INITIALIZING ARGUMENTS
      string filename = args[0];
      int arg_repeats = int.Parse(args[2]);
      string threadCountsString = args[1].Substring(1, args[1].Length - 2);
      int[] arg_threadCounts = threadCountsString.Split(",").Select(@is => int.Parse(@is)).ToArray();
      string threadCountsBackToString = string.Join(",", arg_threadCounts);
      Console.WriteLine($"Arguments (Filename:{filename}, ThreadCounts:[{threadCountsBackToString}], Repeats:{arg_repeats}");
      // PARSE JSON FILE
      SynTask[] tasks = BenchmarkFile.ReadFromFile(filename);

      Console.WriteLine("Parsing done.");
      int result = Run(tasks: tasks, threadCounts: arg_threadCounts, repeats: arg_repeats);
      return result;
    }

    static int Run(SynTask[] tasks, int[] threadCounts, int repeats)
    {
      StringBuilder errors = new StringBuilder();
      StringBuilder pgfCoordinates = new();
      StringBuilder texTabularData = new();
      Console.WriteLine();
      Console.Write("{0,-12}{1,6}", "Name", "Thrds");
      for (int ri = 1; ri <= repeats; ri++)
        Console.Write("{0,8}", "run " + ri);
      Console.Write("{0,8}", "AVG");
      Console.WriteLine();
      bool theVeryFirstRun = true;
      foreach (SynTask bench in tasks ?? Array.Empty<SynTask>())
      {
        (int, double)[] averages = new (int, double)[threadCounts.Length];
        for (int thci = 0; thci < threadCounts.Length; thci++)
        {
          int thCount = threadCounts[thci];
          Console.Write("{0,-12}{1,6}", bench.Name.Substring(0, Math.Min(12, bench.Name.Length)), thCount);
          double[] durationsRpt = new double[repeats];
          bool succeess = true;
          for (int r = 0; r < repeats; r++)
          {
            GC.Collect();
            Thread.Sleep(WAIT_BETWEEN_RUNS_MS);
          beginning:
            SynthesisJob job = new SynthesisJob(bench.ProgramEnv, new ThreadCount(thCount), SearchOptions.FindOnlyFirstProgram);
            DateTime t0 = DateTime.UtcNow;
            var programs = job.FindPrograms();
            DateTime t1 = DateTime.UtcNow;
            if (theVeryFirstRun)
            {
              theVeryFirstRun = false;
              goto beginning;
            }
            if (programs.Any())
            {
              ProgramEnvironment firstProgram = programs.First()!;
              PrettyStringer ps = new PrettyStringer(firstProgram.NameBindings);
              string foundProgramString = firstProgram.Root.Accept(ps);
              if (foundProgramString == bench.ExpectedProgram)
              {
                durationsRpt[r] = (t1 - t0).TotalSeconds;
                Console.Write("{0,8:F3}", durationsRpt[r]);
              }
              else
              {
                succeess = false;
                Console.Write("{0,8}", "F");
                errors.AppendLine($"({bench.Name}) \n Expecting: {bench.ExpectedProgram} \n Found: {foundProgramString}");
              }
            }
            else
            {
              succeess = false;
              Console.Write("{0,8}", "F");
              errors.Append($"({bench.Name}), Threads {thCount}, Repeat {r + 1}, Program not found.");
            }
          }
          if (succeess)
          {
            double avgRepeats = durationsRpt.Average();
            Console.WriteLine("{0,8:F3}", avgRepeats);
            averages[thci] = (threadCounts[thci], avgRepeats);
          }
          else
          {
            Console.WriteLine("{0,8}", "N/A");
            break;
          }
          GC.Collect();
          Thread.Sleep(WAIT_BETWEEN_TASKS_MS);
        }
        string coordsStr = string.Join(" ", averages.Select(a => $"({a.Item1},{a.Item2:F2})"));
        string dataStr = string.Join(" & ", averages.Select(a => $"{a.Item2:F2}"));
        pgfCoordinates.AppendLine(bench.Name + ": " + coordsStr);
        texTabularData.AppendLine(dataStr);
        Console.WriteLine(bench.Name + ": " + bench.ExpectedProgram);
        Console.WriteLine();
      }
      Console.WriteLine("For PGF:");
      Console.WriteLine(pgfCoordinates);
      Console.WriteLine("For Tabular:");
      Console.WriteLine(texTabularData);
      if (errors.Length != 0)
      {
        Console.WriteLine("Errors:");
        Console.Write(errors);
      }
      Console.WriteLine("Done.");
      return 0;
    }
  }


}