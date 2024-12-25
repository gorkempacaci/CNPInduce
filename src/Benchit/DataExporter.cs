using CNP.Helper;
using System.Text;

namespace Benchit
{
  public class DataExporter
  {
    public Dictionary<string, (int depth, int complexityExponent, int pex, int nex, List<(int threads, double avg, double stdDev)> durations)> results = new();
    public void Add(string programName, int depth, int complexityExponent, int pex, int nex, int threadCount, double[] runs)
    {
      var avg = runs.Average();
      var sDev = Math.Sqrt(runs.Sum(r => Math.Pow(r - avg, 2)) / runs.Length);
      //var sErr = sDev / Math.Sqrt(runs.Count());
      var record = results.GetOrAdd(programName, () => (depth, complexityExponent, pex, nex, new()));
      var list = record.durations;
      list.Add((threads: threadCount, avg:avg, stdDev: sDev));
    }

    private string toBigO(int complexityExponent)
    {
      return complexityExponent switch
      {
        0 => "O(1)",
        1 => "O(n)",
        _ => $"O(n^{complexityExponent})"
      };
    }
    
    public string ExportToTEX()
    {
      StringBuilder sb = new();
      const int nameWidth = -10;
      foreach (var p in results)
      {
        sb.Append($"{p.Key,nameWidth}");
        sb.Append($"& {p.Value.depth}");
        sb.Append($"& ${toBigO(p.Value.complexityExponent)}$");
        sb.Append($"& {p.Value.pex}");
        sb.Append($"& {p.Value.nex}");
        var t1 = p.Value.durations.First();
        var t6 = p.Value.durations.Last();
        sb.Append("& " + t1.avg.ToString("F3") + " $\\pm$" + t1.stdDev.ToString("F3"));
        double speedUp = t1.avg / t6.avg;
        sb.Append("& " + t6.avg.ToString("F3")+ " $\\pm$"  + t6.stdDev.ToString("F3"));
        sb.Append("& " + speedUp.ToString("F1") + "x");
        sb.AppendLine("  \\\\");
      }
      return sb.ToString();
    }
    public string ExportToMarkdown()
    {
      StringBuilder sb = new();
      sb.AppendLine("| Name       | AST Depth | Complexity | Ex+ | Ex- | Single-threaded | Multi-threaded | Speedup | ");
      sb.AppendLine("| ---------- | --------- | ---------- | --- | --- | --------------- | -------------- | ------- | ");
      foreach (var p in results)
      {
        sb.Append($"| {p.Key,-10}");
        sb.Append($" | {p.Value.depth,9}");
        sb.Append($" | {toBigO(p.Value.complexityExponent),10}");
        sb.Append($" | {p.Value.pex,3}");
        sb.Append($" | {p.Value.nex,3}");
        var t1 = p.Value.durations.First();
        var t6 = p.Value.durations.Last();
        sb.Append($" | {t1.avg.ToString("F3") + " ±" + t1.stdDev.ToString("F3"),15}");
        sb.Append($" | {t6.avg.ToString("F3") + " ±" + t6.stdDev.ToString("F3"),14}");
        double speedUp = t1.avg / t6.avg;
        sb.Append($" | {speedUp.ToString("F1"),7}");
        sb.AppendLine(" |");
      }
      return sb.ToString();
    }
  }


}