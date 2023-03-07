using System;
using CNP;
using CNP.Helper;

namespace CNP.Language
{
  /// <summary>
  /// Immutable
  /// </summary>
  public struct ConstantTerm : ITerm
  {
    public readonly object Value;

/// <summary>
/// Term value can only be string or int.
/// </summary>
    public ConstantTerm(object o)
    {
      if (o is string || o is int)
        Value = o;
      else throw new ArgumentOutOfRangeException("Constantterm can be int or string.");
    }

    public ConstantTerm(ConstantTerm other)
    {
      this.Value = other.Value;
    }

    public override int GetHashCode()
    {
      return Value.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      if (!(obj is ConstantTerm c))
        return false;
      return this.Value.Equals(c.Value);
    }

    public bool IsGround() => true;
    public bool Contains(Free other) => false;

    public string Accept(ICNPVisitor ps)
    {
      return ps.Visit(this);
    }

    public ITerm Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public ITerm GetFreeReplaced(Free _, ITerm __)
    {
      return this;
    }

    public override string ToString()
    {
      return Value.ToString();
    }
  }
}
