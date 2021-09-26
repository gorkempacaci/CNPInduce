using System;
using System.Collections.Generic;
using CNP.Helper.EagerLinq;
using CNP.Helper;

namespace CNP.Language
{
  /// <summary>
  /// Meta-variable for name for an argument. Can be bound to a ground identifier or can be free.
  /// </summary>
  public class NameVar : Term, IComparable
  {
    private string _name;
    private static int _argNameCounter = 0;

    /// <summary>
    /// When bound to a name, the name has to be different to all those in this list. Helps with checking the names to be different, for example for the operands of the and operator when needed.
    /// </summary>
    public List<NameVar> _nameConstraints = new List<NameVar>();
    public IEnumerable<NameVar> NameConstraints => _nameConstraints;
    public string Name => _name ?? ("#" + _argNameId.ToString());
    public readonly int _argNameId = System.Threading.Interlocked.Increment(ref _argNameCounter);

    private NameVar()
    {
      _name = null;
    }

    public NameVar(string name)
    {
      Validator.AssertArgumentName(name);
      _name = name;
    }

    public void AddNameConstraint(NameVar otherNameVar)
    {
      if (otherNameVar.IsGround() && this.IsGround() && this.Name == otherNameVar.Name)
        throw new Exception("Cannot add a NameVar constraint which is violated to start with.");
      _nameConstraints.Add(otherNameVar);
    }

    public static void AddReciprocalNameConstraints(NameVar n1, NameVar n2)
    {
      n1.AddNameConstraint(n2);
      n2.AddNameConstraint(n1);
    }

    /// <summary>
    /// Adds cross constraints so that every name in list1 has to be differently named to every name in list2 and vice versa,
    /// </summary>
    /// <param name="list1"></param>
    /// <param name="list2"></param>
    public static void AddNameConstraintsAcross(IEnumerable<NameVar> list1, IEnumerable<NameVar> list2)
    {
      Mathes.Cartesian<NameVar,NameVar>(list1, list2, AddReciprocalNameConstraints);
    }

    /// <summary>
    /// Adds name constraints so that each NameVar in this list has to be differently named to every other name in the list.
    /// </summary>
    /// <param name="list"></param>
    public static void AddNameConstraintsWithin(IEnumerable<NameVar> list)
    {
      throw new NotImplementedException();
    }

    public bool NameConstraintsCheck(string name)
    {
      foreach(NameVar foe in NameConstraints)
      {
        if (foe.IsGround() && foe.Name == name)
          return false;
      }
      return true;
    }


    public override bool Contains(Free other)
    {
      return false;
    }


    public override bool IsGround() => _name != null;


    /// <summary>
    /// Returns true if both names are ground and have the same name, or if they're the same object.
    /// </summary>
    public override bool Equals(object obj)
    {
      if (obj is string sName)// && this.IsGround())
      {
        throw new Exception("NameVar can't be compared to string.");
        //return sName == _name;
      }
      else
      if (!(obj is NameVar other))
      {
        return false;
      }
      else if (this.IsGround() && other.IsGround())
      {
        return Name == other.Name;
      }
      else //if (!this.IsGround() && !other.IsGround())
      {
        return object.ReferenceEquals(this, obj);
      }
      //else return false;
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