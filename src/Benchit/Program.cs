
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
    /// <summary>
    /// Example tasks to initialize the structure of the benchmarks.json file.
    /// </summary>
    static BenchmarkFileEntry[] example_tasks =
    {
      new(){ Name="append", ExpectedProgram="foldr(cons, id)", Valence="{b0:in, as:in, bs:out}",
        Examples="{{b0:[4,5,6], as:[1,2,3], bs:[1,2,3,4,5,6]}]"},
      new(){ Name="reverse3", ExpectedProgram="foldl(cons, id)", Valence="{b0:in, as:in, bs:out}",
        Examples="[{b0:[], as:[1,2,3], b:[3,2,1]}]"}

    };

    [RequiresAssemblyFiles()]
    public static int Main(string[] args)
    {
      if (args.Length == 0 || args[0] == "--help")
      {
        Console.WriteLine("Usage: benchit FILENAME MAX_DEPTH THREAD_COUNTS_ARR REPEATS");
        Console.WriteLine("Example: benchit benchmarks.json 4 [1,2,3,4] 3");
        string benchmarks_example = JsonSerializer.Serialize(example_tasks, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine("Example json file:");
        Console.WriteLine(benchmarks_example);
        return 0;
      }
      // PRINTING CNP VERSION
      string cnpTimeString = System.IO.File.GetCreationTime(typeof(IProgram).Assembly.Location).ToString();
      Console.WriteLine("Using CNP: " + cnpTimeString);
      // INITIALIZING ARGUMENTS
      string filename = args[0];
      int arg_max_depth = int.Parse(args[1]);
      int arg_repeats = int.Parse(args[3]);
      string threadCountsString = args[2].Substring(1, args[2].Length - 2);
      int[] arg_threadCounts = threadCountsString.Split(",").Select(@is => int.Parse(@is)).ToArray();
      string threadCountsBackToString = string.Join(",", arg_threadCounts);
      Console.WriteLine($"Arguments (Filename:{filename}, MaxDepth:{arg_max_depth}, ThreadCounts:[{threadCountsBackToString}]");
      // PARSE JSON FILE
      string jsonContent = File.ReadAllText(filename);
      BenchmarkFileEntry[] benchEntries = JsonSerializer.Deserialize<BenchmarkFileEntry[]>(jsonContent) ?? Array.Empty<BenchmarkFileEntry>();
      SynTask[] synTasks = new SynTask[benchEntries.Length];
      Console.WriteLine("Number of benchmarks read: " + synTasks.Length);
      for (int i = 0; i < synTasks.Length; i++)
      {
        try
        {
          synTasks[i] = benchEntries[i].Parse(arg_max_depth); // parse valences and examples into CNP
        }
        catch (Exception e)
        {
          Console.WriteLine("Error while parsing " + benchEntries[i].Name);
          Console.WriteLine(e);
          return -1;
        }
      }
      Console.WriteLine("Parsing done.");
      int result = Run(tasks: synTasks, max_depth: arg_max_depth, threadCounts: arg_threadCounts, repeats: arg_repeats);
      return result;
    }

    static int Run(SynTask[] tasks, int max_depth, int[] threadCounts, int repeats)
    {
      StringBuilder errors = new StringBuilder();
      StringBuilder pgfCoordinates = new();
      StringBuilder texTabularData = new();
      Console.WriteLine();
      Console.Write("|{0,-12}|{1,6}|", "Name", "Thrds");
      for (int ri = 1; ri <= repeats; ri++)
        Console.Write("{0,7}|", "run " + ri);
      Console.Write("{0,7}|", "AVG");
      Console.WriteLine();
      foreach (SynTask bench in tasks ?? Array.Empty<SynTask>())
      {
        (int, double)[] averages = new (int, double)[threadCounts.Length];
        for (int thci = 0; thci < threadCounts.Length; thci++)
        {
          int thCount = threadCounts[thci];
          Console.Write("|{0,-12}|{1,6}|", bench.Name.Substring(0, Math.Min(12, bench.Name.Length)), thCount);
          double[] durationsRpt = new double[repeats];
          bool succeess = true;
          for (int r = 0; r < repeats; r++)
          {
            SynthesisJob job = new SynthesisJob(bench.ProgramEnv, new ThreadCount(thCount), SearchOptions.FindOnlyFirstProgram);
            DateTime t0 = DateTime.UtcNow;
            var programs = job.FindPrograms();
            DateTime t1 = DateTime.UtcNow;
            if (programs.Any())
            {
              ProgramEnvironment firstProgram = programs.First()!;
              PrettyStringer ps = new PrettyStringer(firstProgram.NameBindings);
              string foundProgramString = firstProgram.Root.Pretty(ps);
              if (foundProgramString == bench.ExpectedProgram)
              {
                durationsRpt[r] = (t1 - t0).TotalSeconds;
                Console.Write("{0,7:F2}|", durationsRpt[r]);
              }
              else
              {
                succeess = false;
                Console.Write("{0,7}|", "F");
                errors.AppendLine($"({bench.Name}) \n Expecting: {bench.ExpectedProgram} \n Found: {foundProgramString}");
              }
            }
            else
            {
              succeess = false;
              Console.Write("{0,7}", "F");
              errors.Append($"({bench.Name}), Threads {thCount}, Repeat {r + 1}, Program not found.");
            }
          }
          if (succeess)
          {
            double avgRepeats = durationsRpt.Average();
            Console.WriteLine("{0,7:F2}|", avgRepeats);
            averages[thci] = (threadCounts[thci], avgRepeats);
          } else Console.WriteLine("{0,7}|", "N/A");
        }
        string coordsStr = string.Join(" ", averages.Select(a => $"({a.Item1},{a.Item2:F2})"));
        string dataStr = string.Join(" & ", averages.Select(a => $"{a.Item2:F2}"));
        pgfCoordinates.AppendLine(bench.Name + ": " + coordsStr);
        texTabularData.AppendLine(dataStr);
        Console.WriteLine("---------------------------------------------");
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

  readonly record struct BenchmarkFileEntry
  (string Name, string ExpectedProgram, string Valence, string Examples)
  {
    public SynTask Parse(int searchDepth)
    {
      NameVarBindings names = new();
      FreeFactory frees = new();
      ValenceVar vv = Parser.ParseValence(Valence, names);
      AlphaRelation examples = Parser.ParseAlphaTupleSet(Examples, names, frees);
      ObservedProgram obs = new ObservedProgram(examples, vv, searchDepth, ObservedProgram.Constraint.None);
      ProgramEnvironment env = new ProgramEnvironment(obs, names, frees);
      return new SynTask(Name, ExpectedProgram, env);
    }
  }

  readonly record struct SynTask(
    string Name,
    string ExpectedProgram,
    ProgramEnvironment ProgramEnv
    )
  {}
}