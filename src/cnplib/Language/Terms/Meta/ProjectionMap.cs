using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CNP.Language;
using CNP.Helper.EagerLinq;
using CNP.Helper;

namespace CNP.Language
{
  /// <summary>
  /// Maps source argument names to target argument names.
  /// in a program like proj(id, {a:u, b:v}), represents the {a:u, b:v}. Therefore keys belong to the inner expression and values to the outer one.
  /// </summary>
  public struct ProjectionMap
  {
    public readonly KeyValuePair<NameVar,NameVar>[] Map;

    /// <summary>
    /// Assigns the given array without copying.
    /// </summary>
    public ProjectionMap(KeyValuePair<NameVar,NameVar>[] pairs)
    {
      Map = pairs;
    }

    /// <summary>
    /// Returns the 'source' name when the 'target' name is searched.
    /// </summary>
    public NameVar GetReverse(NameVar o)
    {
      for (int i = 0; i < Map.Length; i++)
      {
        if (Map[i].Value.Index == o.Index)
        {
          return Map[i].Key;
        }
      }
      throw new ArgumentOutOfRangeException();
    }

    public override int GetHashCode()
    {
      return 53;
    }

    public override bool Equals([NotNullWhen(true)] object obj)
    {
      if (obj is ProjectionMap pm)
      {
        if (Map.Length != pm.Map.Length)
          return false;
        for(int i=0; i<Map.Length; i++)
        {
          var OtherTargetNameForI = pm.GetTargetName(Map[i].Key);
          if (OtherTargetNameForI is null || OtherTargetNameForI.Value.Index != Map[i].Value.Index)
            return false;
        }
        return true;
      }
      else return false;
    }

    public string Accept(ICNPVisitor ps)
    {
      return ps.Visit(this);
    }

    public ProjectionMap Clone(CloningContext cc)
    {
      return cc.Clone(this);
    }

    public NameVar? GetTargetName(NameVar from)
    {
      for (int i = 0; i < Map.Length; i++)
      {
        if (Map[i].Key.Index == from.Index)
        {
          return Map[i].Value;
        }
      }
      return null;
    }

    public NameVar this[NameVar from]
    {
      get
      {
        var res = GetTargetName(from);
        if (res.HasValue)
          return res.Value;
        else throw new ArgumentOutOfRangeException();
      }
    }
  }
}
