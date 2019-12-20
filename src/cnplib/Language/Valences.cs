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
            "{a:in, b:*}",
            "{a:*, b:in}"
        });

        public static readonly IEnumerable<Signature> Cons = parse_1(new List<string>
        {
            "{a:in, b:in, ab:*}",
            "{a:*, b:*, ab:in}"
        });

        public static readonly IReadOnlyDictionary<Signature, IEnumerable<OperatorCombinedSignature>> FoldR = parse1_2(
            new List<string>
            {
                "{b0:in, as:in, b:out} / {a:*, b:*, ab:out} / {a:*, b:out}",
                "{b0:out, as:in, b:out} / {a:*, b:*, ab:out} / {a:out, b:out}",
                "{b0:in, as:out, b:out} / {a:out, b:*, ab:out} / {a:*, b:out}",
                "{b0:out, as:out, b:out} / {a:out, b:*, ab:out} / {a:out, b:out}"
            });

        public static readonly IReadOnlyDictionary<Signature, IEnumerable<OperatorCombinedSignature>> FoldL = parse1_2(
            new List<string>
            {
                "{a0:in, bs:in, a:out} / {a:*, b:*, ab:out} / {a:*, b:out}",
                "{a0:out, bs:in, a:out} / {a:*, b:*, ab:out} / {a:out, b:out}",
                "{a0:in, bs:out, a:out} / {a:out, b:*, ab:out} / {a:*, b:out}",
                "{a0:out, bs:out, a:out} / {a:out, b:*, ab:out} / {a:out, b:out}"
            });

        private static Dictionary<Signature, IEnumerable<OperatorCombinedSignature>> parse1_2(
            IEnumerable<string> collapsedSigs)
        {
            var expanded = collapsedSigs.SelectMany(expandAsteriskToInOut);
            var distinctStrs = expanded.Distinct();
                
            var splitSigs = distinctStrs.Select(s => s.Split('/')
                .Select(Parser.ParseProgramSignature)
                .ToArray())
            .Select(sa => new OperatorCombinedSignature(sa[0], sa[1], sa[2]))
            .GroupBy(cs => cs.OperatorSignature)
            .ToDictionary(csg => csg.Key, csg => csg as IEnumerable<OperatorCombinedSignature>);
            return splitSigs;
        }

        private static IEnumerable<Signature> parse_1(IEnumerable<string> sigs)
        {
            return sigs.SelectMany(expandAsteriskToInOut).Distinct().Select(Parser.ParseProgramSignature);
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
