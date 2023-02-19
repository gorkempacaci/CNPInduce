using System;
using System.Collections.Generic;

using CNP.Helper;


namespace CNP.Language
{
  public interface IProgram
  {
    private const Mode In = Mode.In;
    private const Mode Out = Mode.Out;

    /// <summary>
    /// true only if a program tree does not have any program variables (instances of ObservedProgram) in it. A program may be closed and still have some NameVar instances free. (some domain names not ground).
    /// </summary>
    bool IsClosed { get; }

    public sealed string ToString()
    {
      string ps = Pretty(new PrettyStringer(PrettyStringer.Options.Contextless));
      return ps;
    }
    internal ObservedProgram FindHole() => FindRootmostHole().Item1;

    /// <summary>
    /// Replaces the given free with the given term recursively in all subprograms and terms/subterms.
    /// </summary>
    void ReplaceFree(Free free, ITerm term);

    string Pretty(PrettyStringer ps);

    IProgram Clone(CloningContext cc);

    /// <summary>
    /// Returns the first ObservedProgram in the subtree, first as in in-order, LNR search.
    /// If there is no hole, returns null.
    /// </summary>
    ObservedProgram FindLeftmostHole();

    /// <summary>
    /// Returns the first ObservedProgram in the subtree, goes with the one closest to root. Returns the program and the depth it was found (root=0)
    /// </summary>
    /// <returns></returns>
    (ObservedProgram, int) FindRootmostHole(int calleesDistanceToRoot);
    (ObservedProgram, int) FindRootmostHole() => FindRootmostHole(0);

    /// <summary>
    /// Returns the height of this program tree. Calculates on demand.
    /// </summary>
    /// <returns></returns>
    int GetHeight();

    /// <summary>
    /// Returns a qualifying string for the type of expression tree. For example, and(p,and(p,p)) is one where p is elementary operators. Contains no spaces.
    /// </summary>
    /// <returns></returns>
    string GetTreeQualifier();

  }

}
