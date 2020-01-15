using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using CNP.Helper;
using CNP.Parsing;

namespace CNP.Language
{
    public static class TypeHelper
    {
        public static TypeStore<TOperatorType> ParseCompactOperatorTypes<TOperatorType>(
            IEnumerable<string> compactStrs) where TOperatorType:ComposedType
        {
            var expandedStrs = compactStrs.SelectMany(expandAsteriskToInOut).Distinct();
            var types = expandedStrs.Select(Parser.ParseOperatorType<TOperatorType>);
            return new TypeStore<TOperatorType>(types);
        }

        public static TypeStore<ProgramType> ParseCompactProgramTypes(IEnumerable<string> compactStrs)
        {
            var expandedStrs = compactStrs.SelectMany(expandAsteriskToInOut).Distinct();
            var types = expandedStrs.Select(Parser.ParseProgramType);
            return new TypeStore<ProgramType>(types);
        }
        
        private static IEnumerable<string> expandAsteriskToInOut(string c)
        {
            int i = c.IndexOf('*');
            if (i == -1)
            {
                return Iterators.Singleton(c);
            }
            else
            {
                string pr = c.Substring(0, i);
                string po = c.Substring(i + 1);
                var withIn = expandAsteriskToInOut(pr + "in" + po);
                var withOut = expandAsteriskToInOut(pr + "out" + po);
                return Enumerable.Concat(withIn, withOut);
            }
        }
    }
    
}
