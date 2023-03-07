using System;
using System.Collections.Generic;
using CNP.Helper;
using System.Linq;

namespace CNP.Language
{
  public class And : IProgram
  {
    public const int AND_MAX_ARITY = 5;

    private readonly static AndValenceSeries AndValences = AndValenceSeries.Generate(AND_MAX_ARITY);

    public readonly IProgram LHOperand, RHOperand;

    public And(IProgram lhOperand, IProgram rhOperand) 
    {
      LHOperand = lhOperand;
      RHOperand = rhOperand;
    }

    /// <summary>
    /// The valence that lead to this program.
    /// </summary>
    public string DebugValenceString { get; set; }
    /// <summary>
    /// The observations that lead to this program.
    /// </summary>
    public string DebugObservationString { get; set; }

    public void ReplaceFree(Free free, ITerm term)
    {
      LHOperand.ReplaceFree(free, term);
      RHOperand.ReplaceFree(free, term);
    }

    public bool IsClosed => LHOperand.IsClosed && RHOperand.IsClosed;

    public string Accept(ICNPVisitor ps)
    {
      return ps.Visit(this);
    }

    public IProgram Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public string GetTreeQualifier()
    {
      return "and(" + LHOperand.GetTreeQualifier() + "," + RHOperand.GetTreeQualifier() + ")";
    }

    public int GetHeight()
    {
      return Math.Max(LHOperand.GetHeight(), RHOperand.GetHeight()) + 1;
    }

    public ObservedProgram FindLeftmostHole()
    {
      return LHOperand.FindLeftmostHole() ?? RHOperand.FindLeftmostHole();
    }

    public (ObservedProgram, int) FindRootmostHole(int calleesDistanceToRoot = 0)
    {
      var lh = LHOperand.FindRootmostHole(calleesDistanceToRoot + 1);
      var rh = RHOperand.FindRootmostHole(calleesDistanceToRoot + 1);
      if (lh.Item2 <= rh.Item2) return lh; else return rh;
    }

    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment origEnv)
    {
      var origObservation = origEnv.Root.FindHole();
      if (origObservation.RemainingSearchDepth < 2)
        return Array.Empty<ProgramEnvironment>();
      if ((origObservation.Constraints & ObservedProgram.Constraint.NotAnd) == ObservedProgram.Constraint.NotAnd)
        return Array.Empty<ProgramEnvironment>();
      Mode[] modes = origObservation.Valence.GetModesOrderedByNames(origObservation.Observables.Names);
      ProtoAndValence[] valences = AndValences.GetValencesForOpMode(modes);
      NameVar[] opNames = origObservation.Observables.Names;
      ProgramEnvironment[] programs = new ProgramEnvironment[valences.Length];
      for(int i=0; i<valences.Length; i++)
      {
        ProtoAndValence protVal = valences[i];
        var (lhNames, lhIndices, lhNamesOfIns, lhNamesOfOuts) = getNamesIndicesModesForNames(protVal.LHModes, opNames);
        var (rhNames, rhIndices, rhNamesOfIns, rhNamesOfOuts) = getNamesIndicesModesForNames(protVal.RHModes, opNames);
        var (lhTuples, rhTuples) = origObservation.Observables.GetCroppedTo2ByIndices(lhIndices, rhIndices);
        int remSearchDepth = origObservation.RemainingSearchDepth - 1;
        var constraint = ObservedProgram.Constraint.NotAnd;
        var lhRel = new AlphaRelation(lhNames, lhTuples);
        var lhVal = new ValenceVar(lhNamesOfIns, lhNamesOfOuts);
        var lhObs = new ObservedProgram(lhRel, lhVal, remSearchDepth, constraint);
        var rhRel = new AlphaRelation(rhNames, rhTuples);
        var rhVal = new ValenceVar(rhNamesOfIns, rhNamesOfOuts);
        var rhObs = new ObservedProgram(rhRel, rhVal, remSearchDepth, constraint);
        var andProg = new And(lhObs, rhObs);
        andProg.DebugValenceString = valences[i].Accept(PrettyStringer.Contextless);
        var onlyLHNames = protVal.OnlyLHIndices.Select(i => opNames[i]).ToArray();
        var onlyRHNames = protVal.OnlyRHIndices.Select(i => opNames[i]).ToArray();
        var prog = origEnv.Clone((origObservation, andProg), (onlyLHNames,onlyRHNames));
        programs[i] = prog;
      }
      return programs;
    }


    private static (short index, Mode mode)[] getModesAsNonNull(Mode?[] modes) =>
  modes.Select((mm, i) => ((short)i, mm)).Where(e => e.mm.HasValue).Select(e => (e.Item1, e.mm.Value)).ToArray();

    private static (NameVar[] names, short[] indices, NameVar[] namesOfIns, NameVar[] namesOfOuts) getNamesIndicesModesForNames(Mode?[] modes, NameVar[] names)
    {
      (short index, Mode mode)[] indicesModes = getModesAsNonNull(modes);
      var namesarr = new NameVar[indicesModes.Length];
      var indicesarr = new short[indicesModes.Length];
      List<NameVar> namesOfIns = new List<NameVar>(indicesModes.Length);
      List<NameVar> namesOfOuts = new List<NameVar>(indicesModes.Length);
      for(int i=0; i<indicesModes.Length; i++)
      {
        namesarr[i] = names[indicesModes[i].index];
        indicesarr[i] = indicesModes[i].index;
        if (indicesModes[i].mode == Mode.In)
          namesOfIns.Add(namesarr[i]);
        else
          namesOfOuts.Add(namesarr[i]);
      }
      return (namesarr, indicesarr, namesOfIns.ToArray(), namesOfOuts.ToArray());
    }



    //public static IEnumerable<ProgramEnvironment> CreateAtFirstHole2(ProgramEnvironment origEnv)
    //{
    //  var origObservation = origEnv.Root.FindHole();
    //  if (origObservation.RemainingSearchDepth<2)
    //    return Array.Empty<ProgramEnvironment>();
    //  if ((origObservation.Constraints & ObservedProgram.Constraint.NotAnd) == ObservedProgram.Constraint.NotAnd)
    //    return Array.Empty<ProgramEnvironment>();
    //  IEnumerable<AndValence> allValenceCombs = AndValence.Generate(origObservation.Valence);
    //  if (!allValenceCombs.Any())
    //    return Array.Empty<ProgramEnvironment>();
    //  List<ProgramEnvironment> programs = new List<ProgramEnvironment>();
    //  foreach(AndValence valComb in allValenceCombs)
    //  {
    //    //NameVar.AddNameConstraintsInPairsIfNeeded(valComb.Names);
    //    var pNames = valComb.LHValence.Ins.Concat(valComb.LHValence.Outs).ToArray();
    //    var pObs = origObservation.Observables.GetCropped(pNames);
    //    var pProg = new ObservedProgram(pObs, valComb.LHValence, origObservation.RemainingSearchDepth-1, ObservedProgram.Constraint.NotAnd);

    //    var qNames = valComb.RHValence.Ins.Concat(valComb.RHValence.Outs).ToArray();
    //    var qObs = origObservation.Observables.GetCropped(qNames);
    //    var qProg = new ObservedProgram(qObs, valComb.RHValence, origObservation.RemainingSearchDepth-1, ObservedProgram.Constraint.NotAnd);

    //    var andProg = new And(pProg, qProg);

    //    var program = origEnv.Clone((origObservation, andProg), (valComb.OnlyLHNames,valComb.OnlyRHNames));
    //    programs.Add(program);
    //  }
    //  return programs;
    //} 
  }
}
