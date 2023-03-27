using System.Collections.Generic;
using System.Linq;

namespace CNP.Language
{
  /// <summary>
  /// In the context of a string[] 'Names', gives which are In and which are Out.
  /// An array of modes gives a valence (with addition of names in CNP).
  /// If a predicate-expression has its In-modes ground, it promises to terminate and its Out-modes to be ground after execution.
  /// Note: it doesn't mean if its In-s aren't ground it won't execute correctly, just that it does not promise to terminate. Therefore every expression built with these valence rules in mind (called well-modedness constraints) then all those expressions have termination guarantee.
  /// </summary>
  public readonly record struct ModeIndices(short[] Ins, short[] Outs)
  {
    public override int GetHashCode() => GroundValence.CalculateValenceModeNumber(Ins.Length, Outs.Length);

    public static ModeIndices IndicesFromArray(Mode?[] modes)
    {
      return IndicesFromArray(modes.Where(m => m.HasValue).Select(m => m.Value).ToArray());
    }

    public static ModeIndices IndicesFromArray(Mode[] modes)
    {
      List<short> inIndices = new();
      List<short> outIndices = new();
      for (short i = 0; i < modes.Length; i++)
      {
        if (modes[i] == Mode.In)
          inIndices.Add(i);
        else outIndices.Add(i);
      }
      return new ModeIndices(inIndices.ToArray(), outIndices.ToArray());
    }
  }

  /// <summary>
  /// Pairs fold's mode array to modes of recursive and base components. For fold to guarantee the
  /// mode array (for termination), the components have to guarantee the given mode arrays.
  /// </summary>
  public readonly record struct FoldModeIndices(ModeIndices FoldModes, ModeIndices RecursiveCaseModes)
  {
    public override int GetHashCode() => GroundValence.CalculateValenceModeNumber(FoldModes.Ins.Length, FoldModes.Outs.Length);
  }

  /// <summary>
  /// Pairs and's mode array to modes of LH and RH components.
  /// </summary>
  public readonly record struct AndModeIndices(ModeIndices AndModes, ModeIndices LHModes, ModeIndices RHModes)
  {
    public override int GetHashCode() => GroundValence.CalculateValenceModeNumber(AndModes.Ins.Length, AndModes.Outs.Length);
  }

}