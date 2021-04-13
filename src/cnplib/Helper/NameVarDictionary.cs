using System;
using System.Collections.Generic;
using CNP.Language;
using CNP.Helper.EagerLinq;

namespace CNP.Helper
{
  /// <summary>
  /// Maintains a mapping of argument names and the argument name variables they're referenced as.
  /// </summary>
  public class NameVarDictionary : Dictionary<string, NameVar>
  {
    public NameVarDictionary() : base()
    {

    }
    public NameVarDictionary(IEnumerable<NameVar> names)
    {
      foreach (NameVar nv in names)
        if (nv.IsGround())
          this.Add(nv.Name, nv);
        else throw new ArgumentException("NameVarDictionary cannot be initialized with unground names.");
    }
    public NameVar GetOrAdd(string domainName)
    {
      if (TryGetValue(domainName, out NameVar v))
      {
        return v;
      }
      else
      {
        NameVar nv = new NameVar(domainName);
        this.Add(domainName, nv);
        return nv;
      }
    }
  }
}
