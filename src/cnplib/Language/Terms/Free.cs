using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CNP.Helper;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
  public struct Free : ITerm
  {
    public readonly int Index;

    public Free()
    {
      throw new Exception("Free initialization should go through the indexed constructor.");
    }

    internal Free(int indx)
    {
      this.Index = indx;
    }

    public bool IsGround() => false;

    public override int GetHashCode()
    {
      return Index;
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
      if (obj is Free that)
      {
        return this.Index == that.Index;
      }
      else return false;
    }

    public ITerm GetFreeReplaced(Free searchedFree, ITerm newTerm)
    {
      if (this.Index == searchedFree.Index)
        return newTerm;
      else return this;
    }

    public ITerm Cloned(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public bool Contains(Free other)
    {
      return this.Index == other.Index;
    }

    public string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    public ITerm Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public override string ToString()
    {
      return "F" + Index;
    }
  }

}
