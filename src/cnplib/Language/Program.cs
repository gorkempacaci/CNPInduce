using System;
using CNP.Helper;


namespace CNP.Language
{
  public abstract class Program
  {
    private Program root;
    /// <summary>
    /// Used for debugging purposes
    /// </summary>
    protected Program _foundingState;

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

    public object DebugTag;

    /// <summary>
    /// Returns a deep copy of this program. Throws if this program is not the root.
    /// </summary>
    /// TODO: During cloning closed components can be copy-shared to reduce memory use and increase cloning efficiency.
    public Program CloneAtRoot()
    {
      return CloneAtRoot(new());
    }

    /// <summary>
    /// Returns a deep copy of this program. Throws if this program is not the root.
    /// </summary>
    public Program CloneAtRoot(TermReferenceDictionary plannedParenthood)
    {
      if (Root != this)
      {
        throw new Exception("Only root programs can be cloned.");
      }
      Program clone = CloneAsSubTree(plannedParenthood);
      clone.SetAllRootsTo(clone);
      return clone;
    }

    /// <summary>
    /// Does not check if it's the Root program, so should be called with care. Cloning a subtree by itself needs maintaining the mapping of variables until the entire context is cloned.
    /// </summary>
    internal Program CloneAsSubTree(TermReferenceDictionary trd)
    {
      var p = CloneNode(trd);
      p.SetFoundingState(this._foundingState);
      p.DebugTag = this.DebugTag;
      return p;
    }

    protected abstract Program CloneNode(TermReferenceDictionary trd);

    /// <summary>
    /// Checks if the object is the root of source tree, and recursively clones or replaces given observation when found.
    /// </summary>
    internal Program CloneAndReplaceObservation(ObservedProgram oldComponent, Program newComponent)
    {
      if (Root != this)
        throw new InvalidOperationException("CloneAndReplaceObservation: Can only clone at the root.");
      var p = CloneAndReplaceObservationAsSubTree(oldComponent, newComponent, new());
      p.SetAllRootsTo(p);
      return p;
    }

    /// <summary>
    /// Recursively clones subtree while replacing given observation if found. Does not check if the node is root, so should be called with care.
    /// </summary>
    internal Program CloneAndReplaceObservationAsSubTree(ObservedProgram oldComponent, Program newComponent, TermReferenceDictionary plannedParenthood)
    {
      var p = CloneAndReplaceObservationAtNode(oldComponent, newComponent, plannedParenthood);
      if (!object.ReferenceEquals(p, newComponent)) // if p is replacing an observation, do not set its founding state to the observation's, which is null.
      {
        p.SetFoundingState(this._foundingState);
        p.DebugTag = this.DebugTag;
      }
      return p;
    }

    /// <summary>
    /// Clones while replacing a given hole (observedprogram) with another component program.  The newComponent needs to come from the same context as this object. If this=oldComponent, returns newComponent. It only clones the specific node, does not recurse.
    /// </summary>
    protected abstract Program CloneAndReplaceObservationAtNode(ObservedProgram oldComponent, Program newComponent, TermReferenceDictionary plannedParenthood);

    ///// <summary>
    ///// newComponent should be closed. Clones while replacing a given hole (observedprogram) with another component program. Can only be called if the Program object is Root.
    ///// </summary>
    //public Program CloneAndReplaceObservationWithClosed(ObservedProgram oldComponent, Program newComponent)
    //{
    //  if (Root != this)
    //  {
    //    throw new Exception("Only root programs can be cloned.");
    //  }
    //  if (!newComponent.IsClosed)
    //  {
    //    throw new Exception("CloneAndReplaceWithClosed: The newComponent has to be closed.");
    //  }
    //  Program clone = InternalCloneAndReplace(new(), oldComponent, newComponent);
    //  return clone;
    //}


    /// <summary>
    /// Sets the root for this and all subprograms recursively. 
    /// </summary>
    public abstract void SetAllRootsTo(Program newRoot);

    /// <summary>
    /// Only used for debugging purposes. Calls to this method are debug-conditional.
    /// </summary>
    public void SetFoundingState(Program p)
    {
      this._foundingState = p;
    }

    /// <summary>
    /// Returns the first ObservedProgram in the subtree, first as in in-order, LNR search.
    /// If there is no hole, returns null.
    /// </summary>
    internal abstract ObservedProgram FindFirstHole();

    /// <summary>
    /// Returns the height of this program tree. Calculates on demand.
    /// </summary>
    /// <returns></returns>
    public abstract int GetHeight();
  }

}
