
using System.Text.Json;
using CNP.Search;
using CNP.Parsing;
using CNP.Language;
using System.Text;
using CNP;
using System.Diagnostics.CodeAnalysis;
using System.Data.Common;

namespace Benchit
{
  /*
   To use all cpu cores, these env variables have to be set.
    set DOTNET_GCCpuGroup=1
    set DOTNET_gcConcurrent=1
    set DOTNET_Thread_UseAllCpuGroups=1
    set COMPlus_Thread_UseAllCpuGroups=1
    set COMPlus_GCCpuGroup=1
    set COMPlus_gcServer=1
   */

  public class Program
  {

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
      try
      {
        string cnpTimeString = System.IO.File.GetCreationTime(typeof(CNP.Language.IProgram).Assembly.Location).ToString();
        Console.WriteLine("Using CNP: " + cnpTimeString);
      } catch (Exception)
      {
        Console.WriteLine("Printing CNP version failed. (possibly single executable)");
      }
      Console.WriteLine("Preinitializing. ");
      SynthesisJob.PreInitialize();
      //Thread.Sleep(1000);
      // INITIALIZING ARGUMENTS
      string filename = args[0];
      int arg_repeats = int.Parse(args[2]);
      string threadCountsString = args[1].Substring(1, args[1].Length - 2);
      int[] arg_threadCounts = threadCountsString.Split(",").Select(@is => int.Parse(@is)).ToArray();
      string threadCountsBackToString = string.Join(",", arg_threadCounts);
      Console.WriteLine($"Arguments (Filename:{filename}, ThreadCounts:[{threadCountsBackToString}], Repeats:{arg_repeats}");
      // PARSE JSON FILE
      SynTask[] tasks = BenchmarkFile.ReadFromFile(filename, out int maxWaitTimeMsBetweenRuns);

      Console.WriteLine("Parsing done.");
      int result = Run(tasks: tasks, threadCounts: arg_threadCounts, repeats: arg_repeats, maxWaitTimeMsBetweenRuns);
      return result;
    }

    static int Run(SynTask[] tasks, int[] threadCounts, int repeats, int maxWaitMSBetweenRuns)
    {
      DataExporter dataExport = new();
      StringBuilder errors = new();
      Console.WriteLine();
      Console.Write("{0,-12}{1,6}", "Name", "Thrds");
      for (int ri = 1; ri <= repeats; ri++)
        Console.Write("{0,8}", "run " + ri);
      Console.Write("{0,8}", "AVG");
      Console.WriteLine();
      bool theVeryFirstRun = true;
      foreach (SynTask bench in tasks ?? Array.Empty<SynTask>())
      {
        var exCount = (bench.ProgramEnv.Root as ObservedProgram)!.Observations[0].Examples.TuplesCount;
        var negExCount = bench.NegativeExamples.TuplesCount;
        Console.WriteLine(bench.Name + ": " + bench.ExpectedPrograms[0] + (bench.ExpectedPrograms.Length > 1 ? ",..." : ""));
        (int, double)[] averages = new (int, double)[threadCounts.Length];
        for (int thci = 0; thci < threadCounts.Length; thci++) //thread count
        {
          int progHeight = -1;
          int progComplexityExponent = -1;
          int thCount = threadCounts[thci];
          Console.Write("{0,-12}{1,6}", bench.Name.Substring(0, Math.Min(12, bench.Name.Length)), thCount);
          double[] durationsRpt = new double[repeats];
          bool succeess = true;

          for (int r = 0; r < repeats; r++)
          {
          beginning:
            SynthesisJob job = new SynthesisJob(bench.ProgramEnv, bench.NegativeExamples, new ThreadCount(thCount), SearchOptions.FindOnlyFirstProgram);
            DateTime t0 = DateTime.UtcNow, t1 = DateTime.UtcNow;
            //Console.WriteLine("\nNum of neg examples:" + bench.NegativeExamples.TuplesCount);
            var programs = job.FindPrograms(p => { t1 = DateTime.UtcNow;
               });
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
              if (bench.ExpectedPrograms.Contains(foundProgramString))
              {
                durationsRpt[r] = (t1 - t0).TotalSeconds;
                progHeight = firstProgram.Root.GetHeight();
                progComplexityExponent = firstProgram.Root.GetComplexityExponent();
                Console.Write("{0,8:F3}", durationsRpt[r]);
              }
              else
              {
                succeess = false;
                Console.Write("{0,8}", "F");
                string expecting = bench.ExpectedPrograms[0] + (bench.ExpectedPrograms.Length > 1 ? "(or similar)" : "");
                errors.AppendLine($"({bench.Name}) \n Expecting: {expecting} \n Found: {foundProgramString}");
              }
            }
            else
            {
              succeess = false;
              Console.Write("{0,8}", "F");
              errors.AppendLine($"({bench.Name}), Threads {thCount}, Repeat {r + 1}, Program not found.");
            }
            GC.Collect(2, GCCollectionMode.Forced, true);
            int howMuchToWait = Math.Min((int)(durationsRpt[r] * 1000 * 3), maxWaitMSBetweenRuns);
            Thread.Sleep(howMuchToWait);
          }
          if (succeess)
          {
            dataExport.Add(bench.Name, progHeight+1, progComplexityExponent, exCount, negExCount, thci, durationsRpt);
            Console.WriteLine("{0,8:F3}", durationsRpt.Average());
          }
          else
          {
            Console.WriteLine("{0,8}", "N/A");
            break;
          }
        }
        string coordsStr = string.Join(" ", averages.Select(a => $"({a.Item1},{a.Item2:F2})"));
        string dataStr = string.Join(" & ", averages.Select(a => $"{a.Item2:F2}"));
        Console.WriteLine();
      }
      if (errors.Length != 0)
      {
        Console.WriteLine("Errors:");
        Console.Write(errors);
      }
      Console.WriteLine("For Tabular:");
      Console.WriteLine(dataExport.ExportToTEX());
      Console.WriteLine();
      Console.WriteLine(dataExport.ExportToMarkdown());
      Console.WriteLine();
      Console.WriteLine("Done.");
      return 0;
    }
  }


}