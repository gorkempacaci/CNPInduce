using System;

namespace CNP.Language
{

  /// <summary>
  /// Determines if each NameVar in an array is free or bound to a string name.
  /// </summary>
  public class NameVarBindings
  {
    /// <summary>
    /// Differences are stored symmetrically (redundantly). So if i is different to j,  Difference[i,j]=Difference[j,i]=true. 
    /// </summary>
    bool[,] Difference;
    internal string[] Names;
    public int NumNameVars { get; private set; }

    public int Capacity => Names.Length;

    public NameVarBindings() : this(10)
    {
    }

    public NameVarBindings(int capacity)
    {
      Difference = new bool[capacity, capacity];
      Names = new string[capacity];
      NumNameVars = 0;
    }

    /// <summary>
    /// Resizes the storage to 150%
    /// </summary>
    private void resizeUp()
    {
      resizeUpTo((int)(Names.Length * 1.5));
    }
    private void resizeUpTo(int newUBound)
    {
      Array.Resize(ref Names, newUBound);
      bool[,] newDiff = new bool[newUBound, newUBound];
      for (int i = 0; i < NumNameVars; i++)
        for (int j = 0; j < NumNameVars; j++)
          newDiff[i, j] = Difference[i, j];
      Difference = newDiff;
    }

    public bool IsDifferent(int indexNV1, int indexNV2)
    {
      return Difference[indexNV1, indexNV2];
    }

    public bool IsDifferent(NameVar nv1, NameVar nv2)
    {
      return Difference[nv1.Index, nv2.Index];
    }

    public void AssertDifferent(NameVar nv1, NameVar nv2)
    {
      Difference[nv1.Index, nv2.Index] = true;
      Difference[nv2.Index, nv1.Index] = true;
    }

    /// <summary>
    /// Adds a free namevar.
    /// </summary>
    public NameVar FreeNameVar()
    {
      return this.AddNameVar(null);
    }

    /// <summary>
    /// Creates a new NameVar object and returns its index. Resizes the NameVar array as necessary.
    /// </summary>
    public NameVar AddNameVar(string name)
    {
      if ((NumNameVars+1) == Capacity)
      {
        resizeUp();
      }
      int newIndex = NumNameVars++;
      Names[newIndex] = name;
      return new NameVar(newIndex);
    }

    /// <summary>
    /// Leaves the env and obs dirty because it may end up binding some names and not others.
    /// </summary>
    public bool TryBindingAllNamesToGround(ValenceVar vv, (string[] ins, string[] outs) groundNames)
    {
      for (int i = 0; i < groundNames.ins.Length; i++)
        if (!TryBindNameVar(vv.Ins[i], groundNames.ins[i]))
          return false;
      for (int i = 0; i < groundNames.outs.Length; i++)
        if (!TryBindNameVar(vv.Outs[i], groundNames.outs[i]))
          return false;
      return true;
    }

    /// <summary>
    /// Returns true if NameVar can be bound to name.
    /// </summary>
    private bool CanBindNameVar(NameVar nv, string name)
    {
      var currVal = GetNameForVar(nv);
      if (currVal == null)
      {
        for(int i=0; i<NumNameVars; i++)
        {
          if (Difference[nv.Index, i])
          {
            if (Names[i] == name)
              return false;
          }
        }
        return true;
      }
      if (currVal == name)
        return true;
      return false;
    }

    /// <summary>
    /// Binds the name var to the new name if it's not already bound.
    /// If it can bind the name, or if it is already bound to the same name, returns true.
    /// If it's bound to a different name, returns false.
    /// If the NameVar is not bound yet but it is supposed to be different to another NameVar that has the name, returns false. 
    /// </summary>
    public bool TryBindNameVar(NameVar nv, string name)
    {
      if (!CanBindNameVar(nv, name))
        return false;
      BindNameVar(nv, name);
      return true;
    }

    /// <summary>
    /// Binds the name of the given NameVar if it's not bound.
    /// If the NameVar is already bound to the same name, returns true.
    /// If the NameVar is already bound to a different name, or if it's not bound but it's constrained to be different to the given name, throws InvalidOperationException.
    /// </summary>
    public void BindNameVar(NameVar nv, string name)
    {
      if (!CanBindNameVar(nv, name))
        throw new InvalidOperationException("NameVar cannot be assigned to " + name);
      if (Names[nv.Index] == null)
      {
        Names[nv.Index] = name;
      }
      else if (Names[nv.Index] != name)
      {
        throw new InvalidOperationException("NameVar already bound.");
      }   
    }

    public bool IsNameVarBound(NameVar nv)
    {
      return Names[nv.Index] != null;
    }

    /// <summary>
    /// Returns the name for the NameVar with given index. If not bound returns null.
    /// </summary>
    public string GetNameForVar(int index)
    {
      return Names[index];
    }

    public string GetNameForVar(NameVar nv)
    {
      return GetNameForVar(nv.Index);
    }

    /// <summary>
    /// Returns an array of name bindings for a given array of vars.
    /// </summary>
    public string[] GetNamesForVars(NameVar[] vars)
    {
      string[] names = new string[vars.Length];
      for (int i = 0; i < names.Length; i++)
        names[i] = GetNameForVar(vars[i]);
      return names;
    }


  }
}
