using System;
using CNP.Display;
using CNP.Helper;


namespace CNP.Language
{
  public abstract class Program : IPrettyStringable
  {
    private Program root;


    /// <summary>
    /// Closed if this program is not, and does not contain, and observation. If it is closed, a founding original (pre-unification) observation is expected for debugging purposes.
    /// </summary>
    protected Program(bool closed)
    {
      IsClosed = closed;
      root = this;
    }

    /// <summary>
    /// If Root=this, then this is the root program for a program tree. If this
    /// program object is not the root of the tree, it won't clone, as that may
    /// break variable bindings.
    /// </summary>
    public Program Root
    {
      get => root;
      protected set => root = value;
    }

    /// <summary>
    /// true only if a program tree does not have any program variables (instances of ObservedProgram) in it. A program may be closed and still have some NameVar instances free. (some domain names not ground).
    /// </summary>
    public bool IsClosed { get; private set; }

    public abstract string Pretty(PrettyStringer ps);

    public sealed override string ToString()
    {
      string ps = Pretty(new PrettyStringer());
      if (Root == this)
        return ps;
      else return "(Contextless) " + ps;
    }

    /// <summary>
    /// Returns a deep copy of this program. Throws if this program is not the root.
    /// </summary>
    /// TODO: During cloning closed components can be copy-shared to reduce memory use and increase cloning efficiency.
    public Program CloneAtRoot()
    {
      return CloneAtRoot(new(), (null,null));
    }
    public Program CloneAtRoot((ObservedProgram, Program) replaceObservation)
    {
      return CloneAtRoot(new(), replaceObservation);
    }

    public Program CloneAtRoot(TermReferenceDictionary plannedParenthood)
    {
      return CloneAtRoot(plannedParenthood, (null, null));
    }

    /// <summary>
    /// Returns a deep copy of this program. Throws if this program is not the root.
    /// </summary>
    public Program CloneAtRoot(TermReferenceDictionary plannedParenthood, (ObservedProgram, Program) replaceObservation)
    {
      if (Root != this)
      {
        throw new Exception("Only root programs can be cloned.");
      }

      Program clone = CloneAsSubTree(plannedParenthood, replaceObservation);
      clone.SetAllRootsTo(clone);
      return clone;
    }

    /// <summary>
    /// Does not check if it's the Root program, so should be called with care. Cloning a subtree by itself needs maintaining the mapping of variables until the entire context is cloned.
    /// </summary>
    internal abstract Program CloneAsSubTree(TermReferenceDictionary plannedParenthood, (ObservedProgram, Program) replaceObservation = default);


    /// <summary>
    /// Sets the root for this and all subprograms recursively. 
    /// </summary>
    public abstract void SetAllRootsTo(Program newRoot);

    /// <summary>
    /// Returns true if all name constraints hold.
    /// </summary>
    public abstract bool NameConstraintsHold();

    internal ObservedProgram FindHole() => FindRootmostHole().Item1;

    /// <summary>
    /// Returns the first ObservedProgram in the subtree, first as in in-order, LNR search.
    /// If there is no hole, returns null.
    /// </summary>
    internal abstract ObservedProgram FindLeftmostHole();

    /// <summary>
    /// Returns the first ObservedProgram in the subtree, goes with the one closest to root. Returns the program and the depth it was found (root=0)
    /// </summary>
    /// <returns></returns>
    internal abstract (ObservedProgram, int) FindRootmostHole(int calleesDistanceToRoot = 0);

    /// <summary>
    /// Returns the height of this program tree. Calculates on demand.
    /// </summary>
    /// <returns></returns>
    public abstract int GetHeight();

    /// <summary>
    /// Returns a qualifying string for the type of expression tree. For example, and(p,and(p,p)) is one where p is elementary operators. No spaces are contained.
    /// </summary>
    /// <returns></returns>
    public abstract string GetTreeQualifier();
  }

}
