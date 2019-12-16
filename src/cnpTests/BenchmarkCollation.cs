using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


    public class BenchmarkCollation
    {
        Dictionary<string, BenchmarkEntry> benchmarks = new Dictionary<string, BenchmarkEntry>();
        public void CollateNewTime(string programName, string programCNPString, double time)
        {
            string key = BenchmarkEntry.KeyFor(programName, programCNPString);
            if (benchmarks.TryGetValue(key, out BenchmarkEntry be))
            {
                be.NewTime(time);
            }
            else
            {
                benchmarks.Add(key, new BenchmarkEntry(programName, programCNPString, time));
            }
        }
        public string ToMarkdown()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Program | CNP | Min | Max");
            sb.AppendLine("--- | --- | ---: | ---:");
            foreach(BenchmarkEntry en in benchmarks.Values)
            {
                sb.AppendLine(string.Format("{0} | {1} | {2:F4}s | {3:F4}s", en.ProgramName.Replace("|", "\\|"), en.ProgramCNPString.Replace("|","\\|"), en.MinTime, en.MaxTime));
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
    public class BenchmarkEntry
    {
        public string ProgramName { get; private set; }
        public string ProgramCNPString { get; private set; }
        public double MinTime { get; private set; }
        public double MaxTime { get; private set; }
        public BenchmarkEntry(string programName, string programCNPString, double firstTime)
        {
            ProgramName = programName;
            ProgramCNPString = programCNPString;
            MinTime = MaxTime = firstTime;
        }
        public override bool Equals(object obj)
        {
            if (obj is null || !(obj is BenchmarkEntry other))
                return false;
            return ProgramName == other.ProgramName && ProgramCNPString == other.ProgramCNPString;
        }
        public override int GetHashCode()
        {
            return CNP.Helper.HashCode.Combined(ProgramName, ProgramCNPString);
        }
        public void NewTime(double time)
        {
            MinTime = Math.Min(MinTime, time);
            MaxTime = Math.Max(MaxTime, time);
        }
        public static string KeyFor(string programName, string programCNPString)
        {
            return programName + "(" + programCNPString + ")";
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
        public void Finish(string programName, string programCNPString)
        {
            if (!_t1.HasValue)
                TakeFinishTime();
            double td = (_t1.Value - _t0).TotalSeconds;
            _target.CollateNewTime(programName, programCNPString, td);
        }
    }

