using System;
using System.Collections.Generic;
using CNP.Language;
using CNP.Helper.EagerLinq;

namespace CNP.Helper
{
  /// <summary>
  /// Maintains a mapping of argument names and the argument name variables they're referenced as.
  /// </summary>
  //public class NameVarDictionary : Dictionary<string, NameVar>
  //{
  //  public NameVarDictionary() : base()
  //  {

  //  }
  //  public NameVarDictionary(IEnumerable<NameVar> names, NameVarBindings nvb)
  //  {
  //    foreach (NameVar nv in names)
  //    {
  //      string name = nvb.GetNameForVar(nv);
  //      if (name is not null)
  //        this.Add(name, nv);
  //      else throw new ArgumentException("NameVarDictionary cannot be initialized with unground names.");
  //    } 
  //  }
  //  public NameVar GetOrAdd(string domainName)
  //  {
  //    return this.GetOrAdd(domainName, () => new NameVar(domainName));
  //  }
  //}
}
