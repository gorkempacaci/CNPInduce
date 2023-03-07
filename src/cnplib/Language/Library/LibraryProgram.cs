using System;
using CNP;
using CNP.Language;

namespace CNP.Language
{
  public abstract class LibraryProgram : IProgram
  {
    /// <summary>
    /// The valence that lead to this program.
    /// </summary>
    public string DebugValenceString { get; set; }
    /// <summary>
    /// The observations that lead to this program.
    /// </summary>
    public string DebugObservationString { get; set; }

    public ObservedProgram FindLeftmostHole() => null;
    public (ObservedProgram, int) FindRootmostHole(int calleesDistanceToRoot) => (null, int.MaxValue);
    public int GetHeight() => 0;
    public string GetTreeQualifier() => "l";
    public void ReplaceFree(Free free, ITerm term) { }
    public bool IsClosed => true;

    public abstract IProgram Clone(CloningContext cc);
    public abstract string Accept(ICNPVisitor ps);
  }
}

