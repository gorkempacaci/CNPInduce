using System;
using System.Collections.Generic;
using CNP.Language;
using CNP.Helper;

namespace CNP.Helper
{
  /// <summary>
  /// Maintains a mapping of variable names and the Frees they're representing, mostly for parsing purposes.
  /// </summary>
  public class FreeDictionary : Dictionary<string, Free>
  {
    FreeFactory fact;
    public FreeDictionary(FreeFactory factory)
    {
      fact = factory;
    }
    public Free GetOrAdd(string variableName)
    {
      return this.GetOrAdd(variableName, () => fact.NewFree());
    }
  }
}
