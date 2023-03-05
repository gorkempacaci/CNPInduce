using System;
using System.Linq;
using CNP.Language;
using CNP.Helper;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CNP.Language
{
  



  /// <summary>
  /// A mapping of op's mode list to lh and rh mode lists without referring to specific NameVars. 
  /// </summary>
  /// <param name="OpModes">Op's mode list indices. Covers the arity. </param>
  /// <param name="LHModes">LH's modes with Op's domain. Null when not bound by LH (op_i \notin names(P))</param>
  /// <param name="RHModes">RH's modes with Op's domain. Null when not bound by RH (op_i \notin names(Q))</param>
  /// <param name="OnlyLHIndices">Indices of positions bound only in LH, not in RH. May not cover the arity.</param>
  /// <param name="OnlyRHIndices">Indices of positions bound only in RH, not in LH. May not cover the arity.</param>
  public record struct ProtoAndValence(Mode[] OpModes, Mode?[] LHModes, Mode?[] RHModes, short[] OnlyLHIndices, short[] OnlyRHIndices)
  {
    // pNames (to crop with), pValence, qNames (to crop with), qValence, {onlyPNames, onlyQNames) (for diff)

    public int PositionalModeNumber => CalculatePositionalModeNumber(OpModes);

    private static readonly int[] twosPow = new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256 };
    // see overload upon modification
    public static int CalculatePositionalModeNumber(Mode[] opModes)
    {
      try
      {
        int num = 0;
        for (int i = 0; i < opModes.Length; i++)
          if (opModes[i] == Mode.Out)
            num += twosPow[i];
        return num;
      }
      catch (IndexOutOfRangeException e)
      {
        throw new NotImplementedException($"Maximum number of arguments is {twosPow.Length}", e);
      }
    }



    //// see overload upon modification
    //public static int CalculatePositionalModeNumber(ModeIndices min)
    //{
    //  try
    //  {
    //    int num = 0;
    //    for (int oi = 0; oi < min.Outs.Length; oi++)
    //      num += twosPow[min.Outs[oi]];
    //    return num;
    //  }
    //  catch (IndexOutOfRangeException e)
    //  {
    //    throw new NotImplementedException($"Maximum number of arguments is {twosPow.Length}", e);
    //  }
    //}

    public ProtoAndValence GetClone()
    {
      return new ProtoAndValence(OpModes.Clone() as Mode[], LHModes.Clone() as Mode?[], RHModes.Clone() as Mode?[], OnlyLHIndices.Clone() as short[], OnlyRHIndices.Clone() as short[]);
    }

  }

  public class AndValenceSeries
  {
    private const Mode IN = Mode.In;
    private const Mode OUT = Mode.Out;

    /// <summary>
    /// First dimension is opMode's arity-1, second dimension is the positionalModeNumber, third dimension is the array of valences.
    /// For example, [in, in] valences are found at [1][CalculatePositionalMode([in,in])][*]
    /// </summary>
    public readonly ProtoAndValence[][][] ProtoValencesByArityAndPositionalModeNumber;

    private AndValenceSeries(ProtoAndValence[][][] valences)
    {
      this.ProtoValencesByArityAndPositionalModeNumber = valences;
    }

    public ProtoAndValence[] GetValencesForOpMode(Mode[] opModeList)
    {
      int arityIndex = opModeList.Length - 1; // for 1-ary opModeList, the list is stored at index 0.
      int modePosIndex = ProtoAndValence.CalculatePositionalModeNumber(opModeList);
      return ProtoValencesByArityAndPositionalModeNumber[arityIndex][modePosIndex];
    }


    /// <summary>
    /// Returns all ProtoAndValence possibilities up to maxArity, indexed by the operator's mode lists PositionalModeIndex.
    /// </summary>
    /// <param name="maxArity"></param>
    /// <returns></returns>
    public static AndValenceSeries Generate(int maxArity)
    {
      var opModeListsByArity = AndOpModeLists(maxArity);      
      ProtoAndValence[][][] allProtoAndValences = new ProtoAndValence[maxArity][][];
      for(int arityIndex=0; arityIndex < opModeListsByArity.Length; arityIndex++)
      {
        allProtoAndValences[arityIndex] = new ProtoAndValence[opModeListsByArity[arityIndex].Length][];
        for (int i = 0; i < opModeListsByArity[arityIndex].Length; i++)
        {
          int modeIndex = ProtoAndValence.CalculatePositionalModeNumber(opModeListsByArity[arityIndex][i]);
#if DEBUG
          // [IN, IN] and [IN, IN, IN, IN] have the same modeIndex
          if (allProtoAndValences[arityIndex][modeIndex] != null)
            throw new ArgumentOutOfRangeException("AndValenceSeries.Generate: one position is overwritten twice in the ProtoAndValence[] array.");
#endif
          allProtoAndValences[arityIndex][modeIndex] = GenerateForSingleOpModeList(opModeListsByArity[arityIndex][i]).ToArray();
        }
      }
      return new AndValenceSeries(allProtoAndValences);
    }

    /// <summary>
    /// Returns an array containing all modeList possibilities.
    /// [Arity-1][ElementIndex][ArgumentIndex]
    /// </summary>
    /// <param name="maxArity"></param>
    /// <returns></returns>
    public static Mode[][][] AndOpModeLists(int maxArity)
    {
      var allModesByArity = new List<IEnumerable<Mode>>[maxArity];
      // for arity=1, it can only be in or out so that's the initial list we build the rest of the arities with.
      allModesByArity[0] = new List<IEnumerable<Mode>>() { new Mode[] { IN }, new Mode[] { OUT } };
      for (int arity = 2; arity <= maxArity; arity++) // 1 already added
      {
        allModesByArity[arity-1] = new List<IEnumerable<Mode>>();
        foreach(var e in allModesByArity[arity-2]) // go through the ones in previous arity
        {
          allModesByArity[arity - 1].Add(e.Concat(new Mode[] { IN }));
          allModesByArity[arity - 1].Add(e.Concat(new Mode[] { OUT }));
        }
      }
      return allModesByArity.Select(a => a.Select(els => els.ToArray()).ToArray()).ToArray();
    }

    /// <summary>
    /// Produces all possible ProtoAndValence instances for a given operator mode list. 
    /// </summary>
    public static IEnumerable<ProtoAndValence> GenerateForSingleOpModeList(Mode[] opModes)
    {
      // Mode[] OpModes, Mode?[] LHModes, Mode?[] RHModes, short[] OnlyLHIndices, short[] OnlyRHIndices
      List<ProtoAndValence> protos = new List<ProtoAndValence>() { new ProtoAndValence(opModes, new Mode?[opModes.Length], new Mode?[opModes.Length], new short[0], new short[0]) };
      
      for(int i=0; i<opModes.Length; i++)
      {
        List<ProtoAndValence> newProtos = new List<ProtoAndValence>(protos.Count() * 2);
        foreach (var prevProto in protos)
        {
          bool isLastStage = i == opModes.Length - 1;
          bool allArgsSoFarOnlyLorR = prevProto.OnlyLHIndices.Length + prevProto.OnlyRHIndices.Length == i;
          if (opModes.Length > 1) // if arity=1, no component can have an argument bound only to that component.
          {
            // if all args so far has been bound to only LH or RH, then this one can't be only-LH or only-RH if it's the last stage
            if (!(isLastStage && allArgsSoFarOnlyLorR))
            {
              // i is only in LH
              if (opModes[i] == Mode.In)
                newProtos.Add(MakeNewProtoAtI(prevProto, i, Mode.In, null));
              newProtos.Add(MakeNewProtoAtI(prevProto, i, Mode.Out, null));
              // i is only in RH
              if (opModes[i] == Mode.In)
                newProtos.Add(MakeNewProtoAtI(prevProto, i, null, Mode.In));
              newProtos.Add(MakeNewProtoAtI(prevProto, i, null, Mode.Out));
            }
          }
          // i is in both p and q
          if (opModes[i] == Mode.In)
          {
            newProtos.Add(MakeNewProtoAtI(prevProto, i, Mode.In, Mode.In));
            newProtos.Add(MakeNewProtoAtI(prevProto, i, Mode.In, Mode.Out));
          }
          newProtos.Add(MakeNewProtoAtI(prevProto, i, Mode.Out, Mode.In));
          newProtos.Add(MakeNewProtoAtI(prevProto, i, Mode.Out, Mode.Out));
        }
        protos = newProtos;
      }
      return protos;
    }

    /// <summary>
    /// Returns copies of the given baseProto where at index i, lhModes and rhModes are manipulated to be lhMode and rhMode, respectively.
    /// </summary>
    private static ProtoAndValence MakeNewProtoAtI(in ProtoAndValence baseProto, int i, Mode? lhMode, Mode? rhMode)
    {
      Mode?[] lhModes = baseProto.LHModes.Clone() as Mode?[];
      Mode?[] rhModes = baseProto.RHModes.Clone() as Mode?[];
      lhModes[i] = lhMode;
      rhModes[i] = rhMode;
      short[] onlyLHIndices = baseProto.OnlyLHIndices.Clone() as short[];
      short[] onlyRHIndices = baseProto.OnlyRHIndices.Clone() as short[];
      if (!lhMode.HasValue && !rhMode.HasValue)
      {
        throw new InvalidOperationException("Neither LH or RH bound.");
      }
      else
      {
        if (lhMode.HasValue && !rhMode.HasValue)
        {
          // only LH bound
          onlyLHIndices = baseProto.OnlyLHIndices.Concat(new[] { (short)i }).ToArray();
          onlyRHIndices = baseProto.OnlyRHIndices.Clone() as short[];
        }
        else if (!lhMode.HasValue && rhMode.HasValue)
        {
          // only RH bound
          onlyLHIndices = baseProto.OnlyLHIndices.Clone() as short[];
          onlyRHIndices = baseProto.OnlyRHIndices.Concat(new[] { (short)i }).ToArray();
        }
        else
        {
          // both LH and RH bound
          onlyLHIndices = baseProto.OnlyLHIndices.Clone() as short[];
          onlyRHIndices = baseProto.OnlyRHIndices.Clone() as short[];
        }
      }
      ProtoAndValence newProto = new ProtoAndValence(baseProto.OpModes, lhModes, rhModes, onlyLHIndices, onlyRHIndices);
      return newProto;
    }

  }
}

