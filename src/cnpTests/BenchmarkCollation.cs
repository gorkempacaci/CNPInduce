using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class BenchmarkCollation
{
  private class BenchmarkEntry
  {
    public readonly string ProgramName;
    public double MinTime { get; private set; }
    public double MaxTime { get; private set; }
    public List<(string, string)> programStringsAndExamples = new();
    public BenchmarkEntry(string programName, string programCNPString, double firstTime)
    {
      ProgramName = programName;
      //cnpStrings = new List<string> { programCNPString };
      MinTime = MaxTime = firstTime;
    }
    public override int GetHashCode()
    {
      return ProgramName.GetHashCode();
    }
    public void NewTime(double time, string programString, string exampleAtus)
    {
      MinTime = Math.Min(MinTime, time);
      MaxTime = Math.Max(MaxTime, time);
      if (programString != null)
      {
        programStringsAndExamples.Add((programString, exampleAtus));
      }
    }
  }

  Dictionary<string, BenchmarkEntry> benchmarks = new Dictionary<string, BenchmarkEntry>();
  public void ReportNewTime(string programName, string programCNPString, string atuplesString, double time)
  {
    if (benchmarks.TryGetValue(programName, out BenchmarkEntry be))
    {
      be.NewTime(time, programCNPString, atuplesString);
    }
    else
    {
      benchmarks.Add(programName, new BenchmarkEntry(programName, programCNPString, time));
    }
  }
  public string ToMarkdown()
  {
    Func<string, string> mdClean = (s) => Regex.Replace(s, @"([|\\*])", @"\$1"); // https://stackoverflow.com/questions/39146164/how-to-escape-string-while-programmatically-generating-markdown-table
    StringBuilder sb = new StringBuilder();
    sb.AppendLine("# Depth:" + TestBase.TEST_SEARCH_DEPTH + ", Threads:" + TestBase.TEST_THREAD_COUNT);
    sb.AppendLine("Name | Program | Examples | Min | Max");
    sb.AppendLine("--- | --- | --- | ---: | ---:");
    foreach (BenchmarkEntry en in benchmarks.Values)
    {
      sb.AppendLine(string.Format("{0} | {1} | {2} | {3:F4}s | {4:F4}s",
          mdClean(en.ProgramName),
          string.Join("<br/>", en.programStringsAndExamples.Select(ss => mdClean(ss.Item1))),
          string.Join("<br/>", en.programStringsAndExamples.Select(ss => mdClean(ss.Item2))),
          en.MinTime,
          en.MaxTime));
    }
    return sb.ToString();
  }
  public BenchmarkMeasurement StartNew()
  {
    return BenchmarkMeasurement.Start(this);
  }
  public void WriteToFile(string filename)
  {
    string md = ToMarkdown();
    File.WriteAllText(filename, md);
  }
}


public class BenchmarkMeasurement
{
  BenchmarkCollation _target;
  DateTime _t0;
  DateTime? _t1 = null;
  BenchmarkMeasurement(BenchmarkCollation target, DateTime start)
  {
    _target = target;
    _t0 = start;
  }
  public static BenchmarkMeasurement Start(BenchmarkCollation target)
  {
    DateTime t = DateTime.UtcNow;
    return new BenchmarkMeasurement(target, t);
  }
  public void TakeFinishTime()
  {
    _t1 = DateTime.UtcNow;
  }
  public void ReportFinish(string programName, string programCNPString, string atusStr)
  {
    if (!_t1.HasValue)
      TakeFinishTime();
    double td = (_t1.Value - _t0).TotalSeconds;
    _target.ReportNewTime(programName, programCNPString, atusStr, td);
  }
}

