using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CNP.Helper;
using CNP.Helper.EagerLinq;


namespace CNP.Language
{
  public abstract class Program
  {
    private Program root;

    /// <summary>
    /// If Root=this, then this is the root program for a program tree. If this
    /// program object is not the root of the tree, it won't clone, as that may
    /// break variable bindings.
    /// </summary>
    public Program Root
    {
      get => root;
      protected set => root = value ?? this;
    }

    public Program()
    {
      root = this;
    }

    /// <summary>
    /// true only if a program tree does not have any program variables (instances of ObservedProgram) in it. A program may be closed and still have some NameVar instances free. (some domain names not ground).
    /// </summary>
    public bool IsClosed { get; protected set; }


    /// <summary>
    /// Returns a deep copy of this program. Throws if this program is not the root.
    /// </summary>
    /// TODO: During cloning closed components can be copy-shared to reduce memory use and increase cloning efficiency.
    public Program Clone()
    {
      if (Root != this)
      {
        throw new Exception("Only root programs can be cloned.");
      }
      Program clone = Clone(new());
      clone.SetAllRootsTo(clone);
      return clone;
    }

    /// <summary>
    /// Does not check if it's the Root program, so should be called with care. Cloning a subtree by itself needs maintaining the mapping of variables until the entire context is cloned.
    /// </summary>
    internal abstract Program Clone(TermReferenceDictionary trd);

    internal Program CloneAndReplaceObservation(ObservedProgram oldComponent, Program newComponent)
    {
      if (Root != this)
        throw new InvalidOperationException("CloneAndReplaceObservation: Can only clone at the root.");
      return CloneAndReplaceObservation(oldComponent, newComponent, new());
    }

    /// <summary>
    /// Clones while replacing a given hole (observedprogram) with another component program. If this=oldComponent, returns newComponent. Does not check if the program is root, so it should be called with care for not independently cloning a section of a source tree. 
    /// </summary>
    internal abstract Program CloneAndReplaceObservation(ObservedProgram oldComponent, Program newComponent, TermReferenceDictionary plannedParenthood);

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
    /// Returns the first non-ground program (ObservedProgram) in the subtree, first as in in-order, LNR search.
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
