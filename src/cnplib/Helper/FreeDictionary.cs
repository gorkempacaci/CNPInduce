using System;
using System.Collections.Generic;
using CNP.Language;

namespace CNP.Helper
{
  /// <summary>
  /// Maintains a mapping of variable names and the Frees they're representing, mostly for parsing purposes.
  /// </summary>
    public class FreeDictionary : Dictionary<string, Free>
    {
        public Free GetOrAdd(string variableName)
        {
            if (TryGetValue(variableName, out Free f))
            {
                return f;
            } else
            {
                Free fNew = new Free();
                this.Add(variableName, fNew);
                return fNew;
            }
        }
    }
}
