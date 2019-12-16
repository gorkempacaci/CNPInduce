using System;
using System.Collections.Generic;
using CNP.Language;

namespace CNP.Helper
{
    public class NamedVariables : Dictionary<string, Free>
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
