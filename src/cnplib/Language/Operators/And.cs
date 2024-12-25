using System;
using System.Collections.Generic;
using CNP.Helper;
using System.Linq;
using System.Threading;

namespace CNP.Language
{
  public class And : IProgram
  {
    public const int AND_MAX_ARITY = 5;

    public static readonly AndValenceSeries AndValences = AndValenceSeries.Generate(AND_MAX_ARITY);

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

    public override int GetHashCode()
    {
      return LHOperand.GetHashCode() + RHOperand.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      return obj is And and &&
        and.LHOperand.Equals(and.RHOperand);
    }

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

    public int GetComplexityExponent()
    {
      return Math.Max(LHOperand.GetComplexityExponent(), RHOperand.GetComplexityExponent());
    }

    public ObservedProgram FindLeftmostHole()
    {
      return LHOperand.FindLeftmostHole() ?? RHOperand.FindLeftmostHole();
    }

    public string[] GetGroundNames(NameVarBindings nvb)
    {
      var lhnames = LHOperand.GetGroundNames(nvb);
      var rhnames = RHOperand.GetGroundNames(nvb);
      var andNames = lhnames.Union(rhnames);
      return andNames.ToArray();
    }

    public RunResult _Run(ExecutionEnvironment env, GroundRelation args)
    {
      var lhNames = LHOperand.GetGroundNames(env.NameBindings);
      var lhIndices = args.GetIndicesOfGroundNames(lhNames, env.NameBindings);
      var leftRel = args.GetCroppedByIndices(lhIndices);
      if (LHOperand.Run(env, leftRel) is RunResult.Success succ)
      {
        var rhNames = RHOperand.GetGroundNames(env.NameBindings);
        var rhIndices = args.GetIndicesOfGroundNames(rhNames, env.NameBindings);
        var rightRel = args.GetCroppedByIndices(rhIndices);
        env.ArgumentStack.Push(leftRel);
        var rightResult = RHOperand.Run(env, rightRel);
        env.ArgumentStack.Pop();
        if (rightResult is RunResult.Success succRight)
          return new RunResult.Success(env);
        else return new RunResult.Fail(env);
      }
      else return new RunResult.Fail(env);
    }


    public static IEnumerable<ProgramEnvironment> CreateAtFirstHole(ProgramEnvironment origEnv)
    {
      var origObservation = origEnv.Root.FindHole();
      if (origObservation.RemainingSearchDepth < 2)
        return Array.Empty<ProgramEnvironment>();
      if ((origObservation.Constraints & ObservedProgram.Constraint.NotAnd) == ObservedProgram.Constraint.NotAnd)
        return Array.Empty<ProgramEnvironment>();
      List<ProgramEnvironment> programs = new List<ProgramEnvironment>();
      var constraint = ObservedProgram.Constraint.NotAnd;
      foreach (var obsOI in origObservation.Observations)
      {
        //var debugInfo = origObservation.Observations[oi].GetDebugInformation(origEnv);
        Mode[] modes = obsOI.Valence.GetModesOrderedByNames(obsOI.Examples.Names);
        ProtoAndValence[] valences = AndValences.GetValencesForOpMode(modes);
        NameVar[] opNames = obsOI.Examples.Names;
        int remSearchDepth = origObservation.RemainingSearchDepth - 1;
        int remUnbound = origObservation.RemainingUnboundArguments;
        foreach (var protVal in valences)
        {
          var (lhNames, lhIndices, lhNamesOfIns, lhNamesOfOuts) = getNamesIndicesModesForNames(protVal.LHModes, opNames);
          var lhRel = obsOI.Examples.GetCroppedByIndices(lhIndices);

          var rhNamesIndicesModesList = protVal.RHModesArr.Select(RHModes => getNamesIndicesModesForNames(RHModes, opNames));
          var rhTuplesList = rhNamesIndicesModesList.Select(rh => obsOI.Examples.GetCroppedByIndices(rh.indices));
          
          var lhVal = new ValenceVar(lhNamesOfIns, lhNamesOfOuts);
          var lhObs = new Observation(lhRel, lhVal);

          var rhRelList = Enumerable.Zip(rhNamesIndicesModesList, rhTuplesList).Select(z => z.Second);
          var rhValList = rhNamesIndicesModesList.Select(nim => new ValenceVar(nim.namesOfIns, nim.namesOfOuts));
          var rhObsList = Enumerable.Zip(rhRelList, rhValList).Select(z => new Observation(z.First, z.Second));

          var lhObsP = new ObservedProgram(new[] { lhObs }, remSearchDepth, remUnbound, constraint);
          var rhObsP = new ObservedProgram(rhObsList.ToArray() , remSearchDepth, remUnbound, constraint);
          var andProg = new And(lhObsP, rhObsP);
          //(andProg as IProgram).SetDebugInformation(debugInfo);
          //BUG after and-valence grouping this name difference bit needs to be done differently

          //var diffs = new List<(NameVar[], NameVar[])>();
          //if (protVal.OnlyLHIndices.Length == protVal.OnlyRHIndices.Length)
          //{
          //  for (int k = 0; k < protVal.OnlyLHIndices.Length; k++)
          //  {
          //    var onlyLHNames = protVal.OnlyLHIndices[k].Select(i => opNames[i]).ToArray();
          //    var onlyRHNames = protVal.OnlyRHIndices[k].Select(i => opNames[i]).ToArray();
          //    diffs.Add((onlyLHNames, onlyRHNames));
          //  }
          //}
          //else throw new InvalidOperationException("LH and RH -only indices length are not the same.");
          var prog = origEnv.Clone((origObservation, andProg)); //, diffs.ToArray());
          programs.Add(prog);
        }
      }
      return programs;
    }


    private static (short index, Mode mode)[] getModesAsNonNull(Mode?[] modes)
    {
      return modes.Select((mm, i) => ((short)i, mm))
                  .Where(e => e.mm.HasValue)
                  .Select(e => (e.Item1, e.mm.Value))
                  .ToArray();
    }

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
  }
}
