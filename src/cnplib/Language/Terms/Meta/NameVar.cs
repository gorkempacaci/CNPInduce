using System;
using CNP.Helper;

namespace CNP.Language
{
  /// <summary>
  /// Meta-variable for name for an argument. Can be bound to a ground identifier or can be free.
  /// </summary>
  public class NameVar : Term, IComparable
  {
    private string _name;
    public readonly int _argNameId = System.Threading.Interlocked.Increment(ref _argNameCounter);
    private static int _argNameCounter = 0;


    public string Name => _name ?? ("#" + _argNameId.ToString());

    private NameVar()
    {
      _name = null;
    }

    public NameVar(string name)
    {
      BindName(name);
    }

    public bool BindName(string name)
    {
      if (IsGround())
        return false;
      else
      {
        Validator.AssertArgumentName(name);
        _name = name;
        return true;
      }
    }

    public override bool IsGround() => _name != null;

    public override bool Contains(Free other)
    {
      return false;
    }

    /// <summary>
    /// Returns true if both names are ground and have the same name, or if they're the same object.
    /// </summary>
    public override bool Equals(object obj)
    {
      if (obj is string sName && this.IsGround())
      {
        return sName == _name;
      }
      else if (!(obj is NameVar other))
      {
        return false;
      }
      else if (this.IsGround() && other.IsGround())
      {
        return Name == other.Name;
      }
      else if (!this.IsGround() && !other.IsGround())
      {
        return object.ReferenceEquals(this, other);
      }
      else return false;
    }

    public override int GetHashCode() => Name.GetHashCode();

    public override string ToString()
    {
#if Debug
      return DebugName;
#else
      return Name;
#endif
    }

    public override NameVar Clone(TermReferenceDictionary plannedParenthood)
    {
      if (IsGround())
        return plannedParenthood.GetOrAdd(this, () => new NameVar(_name)) as NameVar;
      else return plannedParenthood.GetOrAdd(this, () => NameVar.NewUnbound()) as NameVar;
    }

    public int CompareTo(object obj)
    {
      if (!(obj is NameVar otherName))
        throw new Exception("ArgumentName.CompareTo: obj is not ArgumentName");
      if (this.IsGround() && otherName.IsGround())
        return String.Compare(_name, otherName._name, StringComparison.Ordinal);
      else return _argNameId.CompareTo(otherName._argNameId);
    }

    public string DebugName => _name == null ? "_" + _argNameId.ToString() : "_" + _argNameId.ToString() + "(" + _name + ")";

    public static NameVar NewUnbound()
    {
      return new NameVar();
    }
  }
}