using System;
using System.Collections.Generic;
using CNP.Helper.EagerLinq;
using CNP.Helper;
using System.Diagnostics.CodeAnalysis;

namespace CNP.Language
{
  /// <summary>
  /// Meta-variable for name for an argument. Can be bound to a ground identifier or can be free.
  /// </summary>
  public struct NameVar : IPrettyStringable
  {
    public readonly int Index;

    public NameVar(int i)
    {
      Index = i;
#if DEBUG
      if (Index == 10)
      {; }
#endif
    }

    public override int GetHashCode() => Index;

    public override bool Equals(object obj)
    {
      return this.Index == ((NameVar)obj).Index;
    }

    public string Pretty(PrettyStringer ps)
    {
      return ps.PrettyString(this);
    }
    public NameVar Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

#if DEBUG
    public override string ToString()
    {
      return "n" + Index;
    }
#endif


    ///// <summary>
    ///// For a given set of pairs of replacement, returns true if the LH names can be assigned to (replaced with) the RH names. Due to lack of true unification, this is only a partial implementation of name constraint checks. Once the replacements are done, the new names should be check again in-place to see if the constraints still hold.
    ///// </summary>
    //public static bool NameConstraintsAllow(IEnumerable<KeyValuePair<NameVar,NameVar>> pairs)
    //{
    //  bool holds = pairs.All(p => p.Key.NameConstraintsAllow(p.Value));
    //  //return holds;
    //}

    //private static void addReciprocalNameConstraints(NameVar n1, NameVar n2)
    //{
    //  n1.addNameConstraint(n2);
    //  n2.addNameConstraint(n1);
    //}

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
    //public static void AddNameConstraintsInPairsIfNeeded(IEnumerable<NameVar> list)
    //{
    //  if (list.Count() < 2)
    //    return;
    //  foreach (var p in Mathes.Combinations(list, 2))
    //  {
    //    var (n1,n2) = p.ToValueTuple2();
    //    addReciprocalNameConstraints(n1, n2);
    //  }
    //}


  }

 
}