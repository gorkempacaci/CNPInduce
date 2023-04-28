using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using CNP.Language;
using CNP.Parsing;
using CNP.Helper;
using CNP.Search;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using CNP;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Convenience")]
[TestClass]
public class TestBase
{
  public const int TEST_ALLOWED_UNBOUND_ARGS = 1;
  public const int TEST_SEARCH_DEPTH = 4;
  public const int TEST_THREAD_COUNT = 1; // ideal 4 on mbp 2019 8c
  /*  Threads Time to run Tests.Synthesis.Elementary
   *                   D3W under load   D3W D3W-i D3W-iR
      1	      34.50    2.2              1.4 1.5   1.3
      2       25.91    1.5              1.1 1.2   1.1
      3       23.46    1.2              1.1 1.2   1.1
      4       22.00    1.2              1.1 1.2   1.3
      5       21.20
      6       21.20
      7       21.44
      8       21.96                     1.6 1.6   1.4
      16                                          23

                    1t    2t    3t    4t    8t
  W D3 with intlck  2.2   1.5   1.2   1.2   2.1

   */

  #region Shorthand for Language Constructs
  protected static Mode In => Mode.In;
  protected static Mode Out => Mode.Out;
  protected static ITerm list() => new NilTerm();
  protected static ITerm list(params ITerm[] terms) => TermList.FromEnumerable(terms);
  protected static ITerm list(params int[] terms) => TermList.FromEnumerable(Enumerable.Select(terms, t => constterm(t)));
  protected static ITerm list(params string[] terms) => TermList.FromEnumerable(Enumerable.Select(terms, t => constterm(t)));
  protected static ITerm cns(ITerm head, ITerm tail) => new TermList(head, tail);
  protected static ITerm constterm(string s) => new ConstantTerm(s);
  protected static ITerm constterm(int i) => new ConstantTerm(i);
  protected static Id id => new Id();
  protected static Cons cons => new Cons();
  protected static Const constant(NameVar dom, ConstantTerm t) => new Const(dom, t);
  protected static Const constant(NameVar dom, NilTerm t) => new Const(dom, t);
  protected static And and(IProgram p, IProgram q) => new And(p, q);
  protected static FoldR foldr(IProgram rec) => new FoldR(recursiveCase: rec);
  protected static FoldL foldl(IProgram rec) => new FoldL(recursiveCase: rec);
  protected static Proj proj(IProgram source, params (NameVar,NameVar)[] projections) => new Proj(source, new ProjectionMap(projections.Select(tu => new KeyValuePair<NameVar,NameVar>(tu.Item1,tu.Item2)).ToArray()));
  #endregion

  protected ProgramEnvironment buildEnvironment(string domains, string atusStr, int searchDepth, int allowedUnboundArgs)
  {
    NameVarBindings names = new();
    FreeFactory frees = new();
    ValenceVar namesModes = Parser.ParseValence(domains, names);
    AlphaRelation atus = Parser.ParseAlphaTupleSet(atusStr, names, frees);
    ObservedProgram obs = new ObservedProgram(atus, namesModes, searchDepth, allowedUnboundArgs, ObservedProgram.Constraint.None);
    ProgramEnvironment env = new ProgramEnvironment(obs, names, frees);
    return env;
  }

  protected ProgramEnvironment assertFirstResultFor(string domains, string atusStr, string expectedProgramString, int searchDepth=TEST_SEARCH_DEPTH, int allowedUnboundArguments= TEST_ALLOWED_UNBOUND_ARGS)
  {
    ProgramEnvironment preEnv = buildEnvironment(domains, atusStr, searchDepth, allowedUnboundArguments);
    SynthesisJob job = new SynthesisJob(preEnv, new ThreadCount(TEST_THREAD_COUNT), SearchOptions.FindOnlyFirstProgram);
    var programs = job.FindPrograms();
    //Assert.AreEqual(1, programs.Count(), "A program should be found.");
    Assert.IsTrue(programs.Any(), "There should be at least one program.");
    ProgramEnvironment env = programs.First();
    PrettyStringer ps = new PrettyStringer(env.NameBindings);
    DebugPrinter p = new DebugPrinter(env.NameBindings);
    string debugString = env.Root.Accept(p);
    Assert.AreEqual(expectedProgramString, env.Root.Accept(ps), "\n DEBUG: " + debugString);
    return env;
  }

  protected void assertNoResultFor(string domains, string atusStr, int searchDepth=TEST_SEARCH_DEPTH, int allowedUnboundArguments=TEST_ALLOWED_UNBOUND_ARGS)
  {
    ProgramEnvironment env = buildEnvironment(domains, atusStr, searchDepth, allowedUnboundArguments);
    SynthesisJob job = new SynthesisJob(env, new ThreadCount(TEST_THREAD_COUNT), SearchOptions.FindAllPrograms);
    var programs = job.FindPrograms();
    var strings = string.Join("\n", programs.Take(3).Select(p => p.Root.Accept(new PrettyStringer(p.NameBindings))));
    Assert.AreEqual(0, programs.Count(), "A program should not have been found. First 3:\n" + strings);
  }

  private static IEnumerable<Free> freesIn(ITerm t)
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

  protected static string contextless(ITerm term)
  {
    PrettyStringer stringer = new PrettyStringer(VisitorOptions.Contextless);
    return term.Accept(stringer);
  }

  protected static string contextless(ITerm[] terms, string[] colNames)
  {
    PrettyStringer stringer = new PrettyStringer(VisitorOptions.Contextless);
    return stringer.Visit(terms, colNames);
  }
}