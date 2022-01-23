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
    public Free GetOrAdd(string variableName)
    {
      return this.GetOrAdd(variableName, () => new Free());
    }
  }
}
