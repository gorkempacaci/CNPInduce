using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using CNP.Helper;

namespace CNP.Language
{

  public struct NilTerm : ITerm
  {
    public bool IsGround() => true;
    public bool Contains(Free other) => false;

    public ITerm Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    public override int GetHashCode()
    {
      return 144;
    }

    public override bool Equals([NotNullWhen(true)] object _)
    {
      return true;
    }

    public ITerm GetFreeReplaced(Free _, ITerm __)
    {
      return this;
    }

    public override string ToString()
    {
      return "[]";
    }
  }
}