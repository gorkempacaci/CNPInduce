using System;
using System.Collections.Generic;
using CNP.Helper.EagerLinq;
using CNP.Helper;
using CNP.Display;
using System.Diagnostics.CodeAnalysis;

namespace CNP.Language
{
  /// <summary>
  /// Meta-variable for name for an argument. Can be bound to a ground identifier or can be free.
  /// </summary>
  public class NameVar : Term, IComparable
  {
    public class StringComparer : IEqualityComparer<object>, IComparer<NameVar>
    {
      public new bool Equals(object x, object y)
      {
        return (x, y) switch
        {
          (NameVar v, NameVar v2) => v.IsGround()&&v2.IsGround() ? v.Name==v2.Name : v.Equals(v2),
          (NameVar v, string s) => v.Name == s,
          (string s, NameVar v) => v.Name == s,
          (_, _) => throw new ArgumentException("Can only compare NameVar to another NameVar, or to a string.")
        };
      }

      public int GetHashCode([DisallowNull] object obj)
      {
        if (obj is NameVar v)
          if (v.IsGround())
            return v.Name.GetHashCode();
          else return v.GetHashCode();
        else if (obj is string s)
          return s.GetHashCode();
        else throw new ArgumentException("NameVarStringComparer can only compare NameVar and string types.");
      }

      public int Compare(NameVar x, NameVar y)
      {
        throw new NotImplementedException();
      }

      public static readonly NameVar.StringComparer Instance;

      static StringComparer()
      {
        Instance = new NameVar.StringComparer();
      }
    }

    private string _name;
    private bool _nameConstraintsHaveBeenCloned = false;

    /// <summary>
    /// When bound to a name, the name has to be different to all those in this list. Helps with checking the names to be different, for example for the operands of the and operator when needed.
    /// </summary>
    public HashSet<NameVar> _nameConstraints = new();
    public IEnumerable<NameVar> NameConstraints => _nameConstraints;
    public string Name => _name ?? ("_" + base.GetHashCode());
    
    private NameVar()
    {
      _name = null;
    }

    public NameVar(string name)
    {
      Validator.AssertArgumentName(name);
      _name = name;
    }

    private void addNameConstraint(NameVar otherNameVar)
    {
      if (otherNameVar.IsGround() && this.IsGround() && this.Name == otherNameVar.Name)
        throw new Exception("Cannot add a NameVar constraint which is violated to start with.");
      _nameConstraints.Add(otherNameVar);
#if DEBUG
      if (_nameConstraints.Count() > 20)
        throw new ArgumentOutOfRangeException();
#endif
    }

    /// <summary>
    /// Returns true if all name constraints for this name holds.
    /// </summary>
    public bool NameConstraintsHold()
    {
      foreach(NameVar c in NameConstraints)
      {
        if (object.ReferenceEquals(this, c))
          return false;
        if (this.IsGround() && c.IsGround() && this.Name == c.Name)
          return false;
      }
      return true;
    }

    /// <summary>
    /// Returns true if this name can be replaced with the given otherVar.
    /// </summary>
    public bool NameConstraintsAllow(NameVar otherVar)
    {
      foreach(NameVar c in NameConstraints)
      {
        if (object.ReferenceEquals(c, otherVar))
          return false;
        if (c.IsGround() && otherVar.IsGround() && c.Name == otherVar.Name)
        {
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// For a given set of pairs of replacement, returns true if the LH names can be assigned to (replaced with) the RH names. Due to lack of true unification, this is only a partial implementation of name constraint checks. Once the replacements are done, the new names should be check again in-place to see if the constraints still hold.
    /// </summary>
    public static bool NameConstraintsAllow(IEnumerable<KeyValuePair<NameVar,NameVar>> pairs)
    {
      bool holds = pairs.All(p => p.Key.NameConstraintsAllow(p.Value));
      return holds;
    }

    private static void addReciprocalNameConstraints(NameVar n1, NameVar n2)
    {
      n1.addNameConstraint(n2);
      n2.addNameConstraint(n1);
    }

    ///// <summary>
    ///// Adds cross constraints so that every name in list1 has to be differently named to every name in list2 and vice versa,
    ///// </summary>
    ///// <param name="list1"></param>
    ///// <param name="list2"></param>
    //public static void AddNameConstraintsAcross(IEnumerable<NameVar> list1, IEnumerable<NameVar> list2)
    //{
    //  Mathes.Cartesian<NameVar,NameVar>(list1, list2, AddReciprocalNameConstraints);
    //}

    /// <summary>
    /// Adds name constraints so that each NameVar in this list has to be differently named to every other name in the list.
    /// Skips the constraint if both ends are already ground.
    /// </summary>
    /// <param name="list"></param>
    public static void AddNameConstraintsInPairsIfNeeded(IEnumerable<NameVar> list)
    {
      if (list.Count() < 2)
        return;
      foreach (var p in Mathes.Combinations(list, 2))
      {
        var (n1,n2) = p.ToValueTuple2();
        addReciprocalNameConstraints(n1, n2);
      }
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
        return object.ReferenceEquals(this, obj);
    }

    public override int GetHashCode() => _name == null ? base.GetHashCode() : _name.GetHashCode();

    public override string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }

    private NameVar _preClone(TermReferenceDictionary plannedParenthood)
    {
      NameVar newMe;
      if (IsGround())
      {
        newMe = (NameVar)plannedParenthood.GetOrAdd(this, () => new NameVar(_name));
      }
      else
      {
        newMe = (NameVar)plannedParenthood.GetOrAdd(this, () => NameVar.NewUnbound());
      }
      return newMe;
    }

    public override NameVar Clone(TermReferenceDictionary plannedParenthood)
    {
      NameVar newMe = _preClone(plannedParenthood);
      if (!newMe._nameConstraintsHaveBeenCloned)
      {
        var newConstraints = NameConstraints.Select(n => n._preClone(plannedParenthood));
        foreach (NameVar newC in newConstraints)
          newMe.addNameConstraint(newC);
        newMe._nameConstraintsHaveBeenCloned = true;
      }
      return newMe;
    }

    public int CompareTo(object obj)
    {
      if (!(obj is NameVar otherName))
        throw new Exception("ArgumentName.CompareTo: obj is not ArgumentName");
      if (this.IsGround() && otherName.IsGround())
        return String.Compare(_name, otherName._name, StringComparison.Ordinal);
      else return this.GetHashCode().CompareTo(otherName.GetHashCode());
    }

    public static NameVar NewUnbound()
    {
      return new NameVar();
    }
  }
}