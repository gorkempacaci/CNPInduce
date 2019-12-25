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
        public static Dictionary<ProgramType, IEnumerable<TOperatorType>> ParseCompactOperatorTypes<TOperatorType>(
            IEnumerable<string> compactStrs) where TOperatorType:OperatorType
        {
            var expandedStrs = compactStrs.SelectMany(expandAsteriskToInOut).Distinct();
            var types = expandedStrs.Select(Parser.ParseOperatorType<TOperatorType>);
            var grps = types.GroupBy(t => t.ExpressionType);
            var dict = grps.ToDictionary(g => g.Key, g => g as IEnumerable<TOperatorType>);
            return dict;
        }

        public static HashSet<ProgramType> ParseCompactProgramTypes(IEnumerable<string> compactStrs)
        {
            var expandedStrs = compactStrs.SelectMany(expandAsteriskToInOut).Distinct();
            var types = expandedStrs.Select(Parser.ParseProgramType);
            return types.ToHashSet();
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
