using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CNP.Helper;
using CNP.Parsing;
using CNP.Helper.EagerLinq;

namespace CNP.Language
{
    public static class Valences
    {

        public static readonly IEnumerable<Signature> Id = parse_1(new List<string>
        {
            "{a:in, b:in}",
            "{a:in, b:out}",
            "{a:out, b:in}"
        });

        public static readonly IEnumerable<Signature> Cons = parse_1(new List<string>
        {
            "{a:in, b:in, ab:out}",
            "{a:out, b:out, ab:in}"
        });

        public static readonly IReadOnlyDictionary<Signature, IEnumerable<OperatorCombinedSignature>> Fold = parse1_2(
            new List<string>
            {
                "{a0:in, as:in, b:*} / {a:in, b:in, ab:*} / {a:in, b:*}",
                "{a0:*, as:in, b:*} / {a:in, b:in, ab:*} / {a:*, b:*}"
            });


        private static Dictionary<Signature, IEnumerable<OperatorCombinedSignature>> parse1_2(
            IEnumerable<string> collapsedSigs)
        {
            var splitSigs = collapsedSigs.SelectMany(expandAsteriskToInOut)
                .Select(s => s.Split('/')
                    .Select(Parser.ParseProgramSignature)
                    .ToArray())
                .Select(sa => new OperatorCombinedSignature(sa[0], sa[1], sa[2]))
                .GroupBy(cs => cs.OperatorSignature)
                .ToDictionary(csg => csg.Key, csg => csg as IEnumerable<OperatorCombinedSignature>);
            return splitSigs;
        }

        private static IEnumerable<Signature> parse_1(IEnumerable<string> sigs)
        {
            return sigs.Select(Parser.ParseProgramSignature);
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
                return withIn.Concat(withOut);
            }
        }
    }
}
