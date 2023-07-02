using System;
using System.Linq;
using CNP.Language;
using CNP.Helper;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CNP.Language
{

  public record struct ProtoAndValenceSingle(Mode?[] LHModes, Mode?[] RHModes, short[] OnlyLHIndices, short[] OnlyRHIndices) { }

  /// <summary>
  /// A mapping of op's mode list to lh and rh mode lists without referring to specific NameVars. 
  /// </summary>
  /// <param name="OpModes">Op's mode list indices. Covers the arity. </param>
  /// <param name="LHModes">LH's modes with Op's domain. Null when not bound by LH (op_i \notin names(P))</param>
  /// <param name="RHModesList">Multiple RH's modes with Op's domain. Null when not bound by RH (op_i \notin names(Q))</param>
  /// <param name="OnlyLHIndices">Indices of positions bound only in LH, not in RH. May not cover the arity.</param>
  public record struct ProtoAndValence(Mode[] OpModes, Mode?[] LHModes, Mode?[][] RHModesArr, short[][] OnlyLHIndices, short[][] OnlyRHIndices)
  {
    // pNames (to crop with), pValence, qNames (to crop with), qValence, {onlyPNames, onlyQNames) (for diff)

    public int PositionalModeNumber = CalculatePositionalModeNumber(OpModes);

    public readonly int NumberOfLHArguments = getNumberOfNonNullArguments(LHModes);

    private static readonly int[] twosPow = new int[] { 1, 2, 4, 8, 16, 32, 64, 128, 256 };

    private static int getNumberOfNonNullArguments(Mode?[] modes)
    {
      int n = modes.Where(m => m is not null).Count();
      if (n == 0)
        throw new InvalidOperationException();
      return n;
    }
    /// <summary>
    /// Gives a unique number depending on number of arguments and each arguments mode (in or out)
    /// </summary>
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

    public string Accept(PrettyStringer vis)
    {
      return vis.Visit(this);
    }

    public ProtoAndValence GetClone()
    {
      var opModesClone = OpModes.Clone() as Mode[];
      var lhModesClone = LHModes.Clone() as Mode?[];
      var rhModesClone = RHModesArr.Select(rm => rm.Clone() as Mode?[]).ToArray();
      var onlyLHIndicesClone = OnlyLHIndices.Select(olhs => olhs.Clone() as short[]).ToArray();
      var onlyRHIndicesClone = OnlyRHIndices.Select(orhs => orhs.Clone() as short[]).ToArray();
      return new ProtoAndValence(opModesClone, lhModesClone, rhModesClone, onlyLHIndicesClone, onlyRHIndicesClone);
    }

    public static ProtoAndValence FromSingles(Mode[] opModes, Mode?[] LHModes, IEnumerable<ProtoAndValenceSingle> singles)
    {
      if (LHModes.All(m => m is null))
        throw new NotImplementedException();
      List<Mode?[]> rhModes = new();
      List<short[]> onlyLHIndices = new();
      List<short[]> onlyRHIndices = new();
      foreach(var single in singles)
      {
        rhModes.Add(single.RHModes);
        onlyLHIndices.Add(single.OnlyLHIndices.ToArray());
        onlyRHIndices.Add(single.OnlyRHIndices.ToArray());
      }
      return new ProtoAndValence(opModes, LHModes, rhModes.ToArray(), onlyLHIndices.ToArray(), onlyRHIndices.ToArray());
    }
  }

  public readonly struct AndValenceSeries
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
      if (opModeList.Length > And.AND_MAX_ARITY)
        return Array.Empty<ProtoAndValence>();
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
          #region DEBUG
#if DEBUG
          // [IN, IN] and [IN, IN, IN, IN] have the same modeIndex
          if (allProtoAndValences[arityIndex][modeIndex] != null)
            throw new ArgumentOutOfRangeException("AndValenceSeries.Generate: one position is overwritten twice in the ProtoAndValence[] array.");
#endif
          #endregion DEBUG
          var opModes = opModeListsByArity[arityIndex][i];
          var valencesForArityAndModeIndex = GenerateForSingleOpModeList(opModes);
          var sorted = valencesForArityAndModeIndex
            .GroupBy(pavs => pavs.LHModes, new SequenceEqualityComparer<Mode?>())
            .Select(g => ProtoAndValence.FromSingles(opModes, g.Key.ToArray(), g))
            .OrderBy(pav => pav.NumberOfLHArguments);
          allProtoAndValences[arityIndex][modeIndex] = sorted.ToArray();
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
    public static IEnumerable<ProtoAndValenceSingle> GenerateForSingleOpModeList(Mode[] opModes)
    {
      // Mode[] OpModes, Mode?[] LHModes, Mode?[] RHModes, short[] OnlyLHIndices, short[] OnlyRHIndices
      var protos = new List<ProtoAndValenceSingle>()
      {
        new ProtoAndValenceSingle(new Mode?[opModes.Length], new Mode?[opModes.Length], Array.Empty<short>(), Array.Empty<short>())
      };

      
      for(short i=0; i<opModes.Length; i++)
      {
        var newProtos = new List<ProtoAndValenceSingle>();
        foreach (var prevProto in protos)
        {
          bool isLastStage = i == opModes.Length - 1;
          bool allArgsSoFarOnlyLorR = prevProto.OnlyLHIndices.Count() + prevProto.OnlyRHIndices.Count() == i;
          int lArgsSoFar = prevProto.LHModes.Count(m => m is not null);
          int rArgsSoFar = prevProto.RHModes.Count(m => m is not null);
          if (opModes.Length > 1) // if arity=1, no component can have an argument bound only to that component.
          {
            //// if all args so far has been bound to only LH or RH, then this one can't be only-LH or only-RH if it's the last stage
            //if (!(isLastStage && allArgsSoFarOnlyLorR))
            //{
            //  if (opModes[i] == Mode.In)
            //    newProtos.Add(MakeNewProtoAtI(prevProto, i, Mode.In, null));
            //  newProtos.Add(MakeNewProtoAtI(prevProto, i, Mode.Out, null));
            //  if (opModes[i] == Mode.In)
            //    newProtos.Add(MakeNewProtoAtI(prevProto, i, null, Mode.In));
            //  newProtos.Add(MakeNewProtoAtI(prevProto, i, null, Mode.Out));
            //}

            // if we want disjoint operands in and
            if (!isLastStage || (isLastStage && rArgsSoFar != 0))
            {
              if (opModes[i] == Mode.In)
                newProtos.Add(MakeNewProtoAtI(prevProto, i, Mode.In, null));
              newProtos.Add(MakeNewProtoAtI(prevProto, i, Mode.Out, null));
            }
            // i is only in RH
            if (!isLastStage || (isLastStage && lArgsSoFar != 0))
            {
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
    private static ProtoAndValenceSingle MakeNewProtoAtI(in ProtoAndValenceSingle baseModes, short i, Mode? lhMode, Mode? rhMode)
    {
      Mode?[] lhModes = baseModes.LHModes.Clone() as Mode?[];
      Mode?[] rhModes = baseModes.RHModes.Clone() as Mode?[];
      lhModes[i] = lhMode;
      rhModes[i] = rhMode;
      short[] onlyLHIndices, onlyRHIndices;
      switch((lhMode,rhMode))
      {
        case (null, not null):
          onlyLHIndices = baseModes.OnlyLHIndices.Clone() as short[];
          onlyRHIndices = baseModes.OnlyRHIndices.Concat(new[] { i }).ToArray();
          break;
        case (not null, null):
          onlyLHIndices = baseModes.OnlyLHIndices.Concat(new[] { i }).ToArray();
          onlyRHIndices = baseModes.OnlyRHIndices.Clone() as short[];
          break;
        case (not null, not null):
          onlyLHIndices = baseModes.OnlyLHIndices.Clone() as short[];
          onlyRHIndices = baseModes.OnlyRHIndices.Clone() as short[];
          break;
        case (null, null):
          throw new InvalidOperationException("Neither LH or RH bound.");
      }
      return new ProtoAndValenceSingle(lhModes, rhModes, onlyLHIndices, onlyRHIndices);
    }

    /// <summary>
    /// Should have the same length for LHModes and RHModes. Returns the indices where it's not null for only LH, and those that are not null only for RH.
    /// </summary>
    /// <param name="modes"></param>
    /// <returns></returns>
    private static (short[] onlyLHIndices, short[] onlyRHIndices) FindOnlyLHorRHIndices((Mode?[] LHModes, Mode?[]RHModes) modes)
    {
      int length = modes.LHModes.Length;
      List<short> onlyLh = new List<short>(length);
      List<short> onlyRh = new List<short>(length);
      for(short i=0; i<length; i++)
      {
        if (modes.LHModes[i].HasValue && !modes.RHModes[i].HasValue)
          onlyLh.Add(i);
        if (!modes.LHModes[i].HasValue && modes.RHModes[i].HasValue)
          onlyRh.Add(i);
      }
      return (onlyLHIndices: onlyLh.ToArray(), onlyRHIndices: onlyRh.ToArray());
    }

  }
}

