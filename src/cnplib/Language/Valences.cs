using System;
using System.Collections.Generic;
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

        public static readonly IReadOnlyDictionary<Signature, (Signature, Signature)> Fold = parse1_2(new List<(string, string, string)>
        {
            ("{a0:in, as:in, b:out}", "{a:in, b:in, ab:out}", "{a:in, b:out}")
        });
        

        private static IReadOnlyDictionary<Signature, (Signature, Signature)> parse1_2(IEnumerable<(string, string, string)> sigs)
        {
            return sigs.ToDictionary(cv => Parser.ParseProgramSignature(cv.Item1),
                cv => (Parser.ParseProgramSignature(cv.Item2), Parser.ParseProgramSignature(cv.Item3)));
        }

        private static IEnumerable<Signature> parse_1(IEnumerable<string> sigs)
        {
            return sigs.Select(Parser.ParseProgramSignature);
        }
    }
}
