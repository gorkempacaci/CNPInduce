using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using CNP.Language;
using CNP.Parsing;
using CNP.Helper;
using CNP.Search;
using CNP.Helper.EagerLinq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Convenience")]
[TestClass]
public class TestBase
{

  #region Shorthand for Language Constructs
  protected static Term list() => NilTerm.Instance;
  protected static Term list(params Term[] terms) => TermList.FromEnumerable(terms);
  protected static Term list(params int[] terms) => TermList.FromEnumerable(Enumerable.Select(terms, t => cnst(t)));
  protected static Term list(params string[] terms) => TermList.FromEnumerable(Enumerable.Select(terms, t => cnst(t)));
  protected static Term cns(Term head, Term tail) => new TermList(head, tail);
  protected static ConstantTerm cnst(string s) => new ConstantTerm(s);
  protected static ConstantTerm cnst(int i) => new ConstantTerm(i);
  protected static Id id => new Id();
  protected static Cons cons => new Cons();
  protected static FoldR foldr(Program rec, Program bas) => new FoldR(recursiveCase: rec, baseCase: bas);
  protected static FoldL foldl(Program rec, Program bas) => new FoldL(recursiveCase: rec, baseCase: bas);
  protected static Proj proj(Program source, params (string, string)[] projections) => new Proj(source, new ProjectionMap(projections.Select(p => (new NameVar(p.Item1), new NameVar(p.Item2)))));
  #endregion

  static readonly BenchmarkCollation benchmark = new BenchmarkCollation();

  protected static string nietBruijnString(IEnumerable<AlphaTuple> ts)
  {
    return string.Join(", ", nietBruijn(ts));
  }

  protected static IEnumerable<AlphaTuple> nietBruijn(IEnumerable<AlphaTuple> ts)
  {
    TermReferenceDictionary trd = new();
    var newTs = ts.Select(t => t.Clone(trd));
    ReplaceFreesWithLambdaStrings(GetDistinctNewFreesIn(trd));
    return newTs;
  }

  /// <summary>
  /// Converts frees _1, _2 to strings λ0, λ1,... where the number is the order of variables.
  /// </summary>
  protected static AlphaTuple nietBruijn(AlphaTuple atu)
  {
    TermReferenceDictionary trd = new();
    atu = atu.Clone(trd);
    ReplaceFreesWithLambdaStrings(GetDistinctNewFreesIn(trd));
    return atu;
  }

  protected static Term nietBruijnTerm(Term t)
  {
    TermReferenceDictionary trd = new();
    t = t.Clone(trd);
    ReplaceFreesWithLambdaStrings(GetDistinctNewFreesIn(trd));
    return t;
  }


  private static IEnumerable<Free> GetDistinctNewFreesIn(TermReferenceDictionary trd)
  {
    return trd.Values.Where(t => t is Free).Select(t => t as Free);
  }

  private static void ReplaceFreesWithLambdaStrings(IEnumerable<Free> distinctFrees)
  {
    distinctFrees.For((Free f, int i) => f.ReplaceInAllContexts(new ConstantTerm("λ" + i.ToString())));
  }

  protected void assertFirstResultFor(string domains, string atusStr, Program elementaryProgramExpected, string programName)
  { 
    NameVarDictionary namevars = new();
    Valence namesModes = Parser.ParseNameModeMap(domains, namevars);
    IEnumerable<AlphaTuple> atus = Parser.ParseAlphaTupleSet(atusStr, namevars);
    SynthesisJob job = new SynthesisJob(atus, namesModes);
    var measurement = benchmark.StartNew();
    var programs = job.FindAllPrograms();
    measurement.TakeFinishTime();
    //Assert.AreEqual(1, programs.Count(), "A program should be found.");
    Assert.AreEqual(elementaryProgramExpected, programs.First());
    measurement.ReportFinish(programName, elementaryProgramExpected.ToString());
  }

  protected void assertNoResultFor(string domains, string atusStr)
  {
    NameVarDictionary namevars = new();
    Valence namesModes = Parser.ParseNameModeMap(domains, namevars);
    IEnumerable<AlphaTuple> atus = Parser.ParseAlphaTupleSet(atusStr, namevars);
    SynthesisJob job = new SynthesisJob(atus, namesModes);
    var programs = job.FindAllPrograms();
    Assert.AreEqual(0, programs.Count(), "A program should not have been found.");
  }

  private static IEnumerable<Free> freesIn(Term t)
  {
    if (t is NilTerm)
      return Iterators.Empty<Free>();
    if (t is ConstantTerm)
    {
      return Iterators.Empty<Free>();
    }
    if (t is TermList tList)
    {
      return freesIn(tList.Head).Concat(freesIn(tList.Tail));
    }
    if (t is Free tFree)
    {
      return Iterators.Singleton(tFree);
    }
    throw new Exception("freesIn: Term not recognized:" + t.ToString());
  }

  [AssemblyCleanup]
  public static void WriteBenchmarksToFile()
  {
    string dt = DateTime.Now.ToString("yyyyMMddT_HHmmss");
    string fn = Path.Combine(Directory.GetCurrentDirectory(), "../../../../benchmarks/run_" + dt + ".md");
    benchmark.WriteToFile(fn);
  }
}